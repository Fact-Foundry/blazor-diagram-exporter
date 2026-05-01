const HEADER_HEIGHT = 36;
const SECTION_LABEL_HEIGHT = 24;
const ICON_SIZE = 14;
const FONT_SIZE = 13;
const HEADER_FONT_SIZE = 14;
const LABEL_PADDING_X = 10;
const LABEL_PADDING_Y = 4;
const ARROW_SIZE = 10;
const DIAMOND_SIZE = 10;

function parseSnapshot(jsonStr) {
  return JSON.parse(jsonStr);
}

function escapeXml(str) {
  return str.replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&apos;");
}

function buildSvg(snapshot) {
  const fontFamily = snapshot.options?.fontFamily ?? "Arial";
  const bg = snapshot.options?.backgroundColor ?? "#ffffff";
  const w = snapshot.canvasWidth;
  const h = snapshot.canvasHeight;
  const scale = snapshot.options?.scale ?? 1;
  const svgW = w * scale;
  const svgH = h * scale;

  const parts = [];
  parts.push(`<svg xmlns="http://www.w3.org/2000/svg" width="${svgW}" height="${svgH}" viewBox="0 0 ${w} ${h}" font-family="${escapeXml(fontFamily)}">`);

  // Background
  parts.push(`<rect width="${w}" height="${h}" fill="${escapeXml(bg)}"/>`);

  // Grid
  const grid = snapshot.options?.grid;
  if (grid && grid.enabled) {
    const spacing = grid.spacing ?? 20;
    const color = grid.color ?? "rgba(255,255,255,0.12)";
    const lineWidth = grid.lineWidth ?? 1;
    parts.push(`<defs><pattern id="grid" width="${spacing}" height="${spacing}" patternUnits="userSpaceOnUse">`);
    parts.push(`<path d="M ${spacing} 0 L 0 0 0 ${spacing}" fill="none" stroke="${escapeXml(color)}" stroke-width="${lineWidth}"/>`);
    parts.push(`</pattern></defs>`);
    parts.push(`<rect width="${w}" height="${h}" fill="url(#grid)"/>`);
  }

  // Links (drawn first, behind nodes)
  for (const link of snapshot.links ?? []) {
    parts.push(buildLinkSvg(snapshot, link));
  }

  // Nodes
  for (const node of snapshot.nodes ?? []) {
    parts.push(buildNodeSvg(node, fontFamily));
  }

  // Link labels (on top, only if using the old label style)
  for (const link of snapshot.links ?? []) {
    const ri = link.renderInfo;
    if (ri.label) {
      parts.push(buildLinkLabelSvg(snapshot, link, fontFamily));
    }
  }

  parts.push(`</svg>`);
  return parts.join("\n");
}

function buildNodeSvg(node, fontFamily) {
  const ri = node.renderInfo;
  const x = node.x;
  const y = node.y;
  const w = node.width;
  const h = node.height;
  const r = ri.borderRadius ?? 8;
  const parts = [];

  parts.push(`<g>`);

  // Body rect
  parts.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${r}" ry="${r}" fill="${escapeXml(ri.bodyColor ?? "#ffffff")}" stroke="${escapeXml(ri.borderColor ?? "#cccccc")}" stroke-width="1"/>`);

  // Header rect (simple rect, bottom corners covered by body)
  parts.push(`<rect x="${x + 0.5}" y="${y + 0.5}" width="${w - 1}" height="${HEADER_HEIGHT}" rx="${r}" ry="${r}" fill="${escapeXml(ri.headerColor ?? "#4a6fa5")}"/>`);
  // Cover bottom corners of header with a small fill rect
  if (HEADER_HEIGHT < h) {
    parts.push(`<rect x="${x + 0.5}" y="${y + HEADER_HEIGHT - r}" width="${w - 1}" height="${r}" fill="${escapeXml(ri.headerColor ?? "#4a6fa5")}"/>`);
  }

  // Header text
  parts.push(`<text x="${x + 10}" y="${y + HEADER_HEIGHT / 2}" fill="${escapeXml(ri.headerTextColor ?? "#ffffff")}" font-size="${HEADER_FONT_SIZE}" font-weight="bold" dominant-baseline="central">${escapeXml(ri.headerText ?? "")}</text>`);

  let currentY = y + HEADER_HEIGHT;

  // Body text
  if (ri.bodyText) {
    const bodyPadding = 8;
    parts.push(`<text x="${x + 10}" y="${currentY + bodyPadding + FONT_SIZE / 2}" fill="${escapeXml(ri.bodyTextColor ?? "#555555")}" font-size="${FONT_SIZE}" dominant-baseline="central">${escapeXml(ri.bodyText)}</text>`);
    currentY += bodyPadding * 2 + FONT_SIZE;
  }

  // Sections
  if (ri.sections) {
    for (const section of ri.sections) {
      if (section.sectionLabel) {
        parts.push(`<line x1="${x}" y1="${currentY}" x2="${x + w}" y2="${currentY}" stroke="${escapeXml(ri.borderColor ?? "#cccccc")}" stroke-width="1"/>`);
        parts.push(`<text x="${x + 10}" y="${currentY + SECTION_LABEL_HEIGHT / 2}" fill="#9ca3af" font-size="${FONT_SIZE - 1}" font-weight="bold" dominant-baseline="central">${escapeXml(section.sectionLabel)}</text>`);
        currentY += SECTION_LABEL_HEIGHT;
      }

      if (!section.rows) continue;

      for (const row of section.rows) {
        const rowH = row.rowHeight ?? 28;

        if (row.rowBackgroundColor) {
          parts.push(`<rect x="${x}" y="${currentY}" width="${w}" height="${rowH}" fill="${escapeXml(row.rowBackgroundColor)}"/>`);
        }

        const textY = currentY + rowH / 2;
        const textColor = row.textColor ?? "#1f2937";
        const portColor = ri.headerColor ?? "#776be7";
        const portBorderColor = ri.bodyColor ?? "#373740";
        const portR = 5;
        const rowPad = 6;
        const gap = 6;
        const portDiam = portR * 2 + 2;

        // Left port dot
        if (row.portId) {
          parts.push(`<circle cx="${x + rowPad + portR}" cy="${textY}" r="${portR}" fill="${escapeXml(portColor)}" stroke="${escapeXml(portBorderColor)}" stroke-width="2"/>`);
        }

        // Right port dot
        if (row.portId) {
          parts.push(`<circle cx="${x + w - rowPad - portR}" cy="${textY}" r="${portR}" fill="${escapeXml(portColor)}" stroke="${escapeXml(portBorderColor)}" stroke-width="2"/>`);
        }

        // Layout: pad + port + gap + icon + gap + label ... type + gap + port + pad
        let textX = x + rowPad + (row.portId ? portDiam + gap : 0);
        const rightEdge = x + w - rowPad - (row.portId ? portDiam + gap : 0);

        // Icon as SVG path
        if (row.icon && row.icon.length > 2 && /^[Mm]/.test(row.icon)) {
          parts.push(`<svg x="${textX}" y="${textY - ICON_SIZE / 2}" width="${ICON_SIZE}" height="${ICON_SIZE}" viewBox="0 0 24 24"><path d="${row.icon}" fill="${escapeXml(textColor)}"/></svg>`);
          textX += ICON_SIZE + gap;
        } else if (row.icon) {
          parts.push(`<text x="${textX}" y="${textY}" fill="${escapeXml(textColor)}" font-size="${ICON_SIZE}" dominant-baseline="central">${escapeXml(row.icon)}</text>`);
          textX += ICON_SIZE + gap;
        }

        // Label
        parts.push(`<text x="${textX}" y="${textY}" fill="${escapeXml(textColor)}" font-size="${FONT_SIZE}" dominant-baseline="central">${escapeXml(row.label ?? "")}</text>`);

        // Secondary text — right-aligned, inside the right port
        if (row.secondaryText) {
          parts.push(`<text x="${rightEdge}" y="${textY}" fill="#9ca3af" font-size="10" dominant-baseline="central" text-anchor="end" style="text-transform:uppercase">${escapeXml(row.secondaryText)}</text>`);
        }

        currentY += rowH;
      }
    }
  }

  parts.push(`</g>`);
  return parts.join("\n");
}

function resolvePortY(snapshot, nodeId, portId) {
  const node = snapshot.nodes.find(n => n.id === nodeId);
  if (!node) return 0;

  if (!portId) return node.y + node.height / 2;

  const ri = node.renderInfo;

  if (ri.portYOffsets && ri.portYOffsets[portId] != null) {
    return node.y + ri.portYOffsets[portId];
  }

  let y = HEADER_HEIGHT;
  if (ri.sections) {
    for (const section of ri.sections) {
      if (section.sectionLabel) y += SECTION_LABEL_HEIGHT;
      if (section.rows) {
        for (const row of section.rows) {
          if (row.portId === portId || (portId && portId.startsWith(row.portId + "."))) {
            return node.y + y + (row.rowHeight ?? 28) / 2;
          }
          y += row.rowHeight ?? 28;
        }
      }
    }
  }

  return node.y + node.height / 2;
}

function resolveEndpoint(snapshot, link, isSource) {
  const nodeId = isSource ? link.sourceNodeId : link.targetNodeId;
  const portId = isSource ? link.sourcePortId : link.targetPortId;
  const otherNodeId = isSource ? link.targetNodeId : link.sourceNodeId;

  const node = snapshot.nodes.find(n => n.id === nodeId);
  const otherNode = snapshot.nodes.find(n => n.id === otherNodeId);
  if (!node) return { x: 0, y: 0 };

  const y = resolvePortY(snapshot, nodeId, portId);

  if (otherNode) {
    const otherCenterX = otherNode.x + otherNode.width / 2;
    const nodeCenterX = node.x + node.width / 2;
    const x = otherCenterX > nodeCenterX ? node.x + node.width : node.x;
    return { x, y };
  }

  return { x: node.x + node.width, y };
}

function evalCubicBezier(p0, p1, p2, p3, t) {
  const mt = 1 - t;
  const mt2 = mt * mt;
  const mt3 = mt2 * mt;
  const t2 = t * t;
  const t3 = t2 * t;
  return {
    x: mt3 * p0.x + 3 * mt2 * t * p1.x + 3 * mt * t2 * p2.x + t3 * p3.x,
    y: mt3 * p0.y + 3 * mt2 * t * p1.y + 3 * mt * t2 * p2.y + t3 * p3.y
  };
}

function evalCubicBezierTangent(p0, p1, p2, p3, t) {
  const mt = 1 - t;
  const mt2 = mt * mt;
  const t2 = t * t;
  return {
    x: 3 * mt2 * (p1.x - p0.x) + 6 * mt * t * (p2.x - p1.x) + 3 * t2 * (p3.x - p2.x),
    y: 3 * mt2 * (p1.y - p0.y) + 6 * mt * t * (p2.y - p1.y) + 3 * t2 * (p3.y - p2.y)
  };
}

function parseSvgPathMidpoint(svgPath, ox, oy) {
  // Parse "M sx sy C c1x c1y, c2x c2y, tx ty" format
  const nums = svgPath.match(/-?[\d.]+/g);
  if (!nums || nums.length < 8) return null;
  const p0 = { x: parseFloat(nums[0]) - ox, y: parseFloat(nums[1]) - oy };
  const p1 = { x: parseFloat(nums[2]) - ox, y: parseFloat(nums[3]) - oy };
  const p2 = { x: parseFloat(nums[4]) - ox, y: parseFloat(nums[5]) - oy };
  const p3 = { x: parseFloat(nums[6]) - ox, y: parseFloat(nums[7]) - oy };
  const mid = evalCubicBezier(p0, p1, p2, p3, 0.5);
  const tan = evalCubicBezierTangent(p0, p1, p2, p3, 0.5);
  const angle = Math.atan2(tan.y, tan.x) * 180 / Math.PI;
  return { x: mid.x, y: mid.y, angle };
}

function buildLinkSvg(snapshot, link) {
  const ri = link.renderInfo;
  const ox = snapshot.offsetX ?? 0;
  const oy = snapshot.offsetY ?? 0;
  const parts = [];

  const strokeColor = ri.strokeColor ?? "#6b7280";
  const strokeWidth = ri.strokeWidth ?? 2;
  const dashArray = (ri.dashPattern && ri.dashPattern.length > 0) ? `stroke-dasharray="${ri.dashPattern.join(",")}"` : "";

  // Always resolve endpoints from node/port positions (where line meets the box)
  const sourcePoint = resolveEndpoint(snapshot, link, true);
  const targetPoint = resolveEndpoint(snapshot, link, false);

  // Draw the path
  if (link.svgPath) {
    parts.push(`<path d="${escapeXml(link.svgPath)}" fill="none" stroke="${escapeXml(strokeColor)}" stroke-width="${strokeWidth}" ${dashArray} stroke-linejoin="round" stroke-linecap="round" transform="translate(${-ox},${-oy})"/>`);
  } else {
    let points;
    if (link.waypoints && link.waypoints.length > 0) {
      points = link.waypoints;
    } else {
      points = [sourcePoint, targetPoint];
    }

    if (points.length >= 2) {
      const d = points.map((p, i) => `${i === 0 ? "M" : "L"}${p.x},${p.y}`).join(" ");
      parts.push(`<path d="${d}" fill="none" stroke="${escapeXml(strokeColor)}" stroke-width="${strokeWidth}" ${dashArray} stroke-linejoin="round" stroke-linecap="round"/>`);
    }
  }

  // Cardinality labels near endpoints ("1" or "*")
  if (ri.sourceLabel) {
    const midX = (sourcePoint.x + targetPoint.x) / 2;
    const offsetX = sourcePoint.x <= midX ? 12 : -12;
    parts.push(`<text x="${sourcePoint.x + offsetX}" y="${sourcePoint.y - 4}" fill="rgba(255,255,255,0.7)" font-size="12" font-weight="bold" text-anchor="middle" dominant-baseline="middle">${escapeXml(ri.sourceLabel)}</text>`);
  }
  if (ri.targetLabel) {
    const midX = (sourcePoint.x + targetPoint.x) / 2;
    const offsetX = targetPoint.x <= midX ? 12 : -12;
    parts.push(`<text x="${targetPoint.x + offsetX}" y="${targetPoint.y - 4}" fill="rgba(255,255,255,0.7)" font-size="12" font-weight="bold" text-anchor="middle" dominant-baseline="middle">${escapeXml(ri.targetLabel)}</text>`);
  }

  // Direction arrow at the visual midpoint of the path
  if (ri.showDirectionArrow) {
    let midX, midY, angle;

    if (link.svgPath) {
      const parsed = parseSvgPathMidpoint(link.svgPath, ox, oy);
      if (parsed) {
        midX = parsed.x;
        midY = parsed.y;
        angle = parsed.angle;
      }
    }

    if (midX == null) {
      // Fallback: use waypoints or endpoint midpoint
      if (link.waypoints && link.waypoints.length > 2) {
        const midIdx = Math.floor(link.waypoints.length / 2);
        midX = link.waypoints[midIdx].x;
        midY = link.waypoints[midIdx].y;
      } else {
        midX = (sourcePoint.x + targetPoint.x) / 2;
        midY = (sourcePoint.y + targetPoint.y) / 2;
      }
      angle = Math.atan2(targetPoint.y - sourcePoint.y, targetPoint.x - sourcePoint.x) * 180 / Math.PI;
    }

    if (ri.isBidirectional) {
      parts.push(`<polygon points="-4,-4 4,0 -4,4" transform="translate(${midX - 8},${midY}) rotate(${angle})" fill="${escapeXml(strokeColor)}"/>`);
      parts.push(`<polygon points="4,-4 -4,0 4,4" transform="translate(${midX + 8},${midY}) rotate(${angle})" fill="${escapeXml(strokeColor)}"/>`);
    } else {
      parts.push(`<polygon points="-6,-4 6,0 -6,4" transform="translate(${midX},${midY}) rotate(${angle})" fill="${escapeXml(strokeColor)}"/>`);
    }
  }

  return parts.join("\n");
}

function buildLinkLabelSvg(snapshot, link, fontFamily) {
  const ri = link.renderInfo;
  if (!ri.label) return "";

  let points;
  if (link.waypoints && link.waypoints.length > 0) {
    points = link.waypoints;
  } else {
    const source = resolveEndpoint(snapshot, link, true);
    const target = resolveEndpoint(snapshot, link, false);
    points = [source, target];
  }

  if (points.length === 0) return "";

  const midIdx = Math.floor(points.length / 2);
  const midX = points[midIdx].x;
  const midY = points[midIdx].y;

  const estWidth = ri.label.length * (FONT_SIZE * 0.6) + LABEL_PADDING_X * 2;
  const rectH = FONT_SIZE + LABEL_PADDING_Y * 2;

  const parts = [];
  parts.push(`<rect x="${midX - estWidth / 2}" y="${midY - rectH / 2}" width="${estWidth}" height="${rectH}" fill="${escapeXml(ri.labelBackgroundColor ?? "#ffffff")}"/>`);
  parts.push(`<text x="${midX}" y="${midY}" fill="${escapeXml(ri.labelColor ?? "#374151")}" font-size="${FONT_SIZE - 1}" text-anchor="middle" dominant-baseline="central">${escapeXml(ri.label)}</text>`);
  return parts.join("\n");
}

async function svgToCanvas(svgStr, w, h) {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => {
      const canvas = document.createElement("canvas");
      canvas.width = w;
      canvas.height = h;
      const ctx = canvas.getContext("2d");
      ctx.drawImage(img, 0, 0, w, h);
      resolve(canvas);
    };
    img.onerror = (e) => reject(new Error("SVG render failed"));
    const blob = new Blob([svgStr], { type: "image/svg+xml;charset=utf-8" });
    img.src = URL.createObjectURL(blob);
  });
}

async function canvasToBytes(canvas) {
  return new Promise((resolve, reject) => {
    canvas.toBlob(blob => {
      if (!blob) { reject(new Error("toBlob failed")); return; }
      blob.arrayBuffer().then(buf => resolve(new Uint8Array(buf))).catch(reject);
    }, "image/png");
  });
}

function triggerDownload(bytes, fileName, mimeType) {
  const blob = new Blob([bytes], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

async function buildMinimalPdf(snapshot) {
  const svgStr = buildSvg(snapshot);
  const scale = snapshot.options?.scale ?? 1;
  const width = Math.ceil(snapshot.canvasWidth * scale);
  const height = Math.ceil(snapshot.canvasHeight * scale);

  const canvas = await svgToCanvas(svgStr, width, height);
  const jpegBytes = await new Promise((resolve, reject) => {
    canvas.toBlob(blob => {
      if (!blob) { reject(new Error("toBlob failed")); return; }
      blob.arrayBuffer().then(buf => resolve(new Uint8Array(buf))).catch(reject);
    }, "image/jpeg", 0.95);
  });

  const enc = new TextEncoder();
  const parts = [];
  const xref = [];

  function write(str) { parts.push(enc.encode(str)); }
  function writeBytes(bytes) { parts.push(bytes); }
  function currentOffset() {
    let total = 0;
    for (const p of parts) total += p.length;
    return total;
  }

  write("%PDF-1.4\n%\xC0\xC1\xC2\xC3\n");

  xref.push(currentOffset());
  write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

  xref.push(currentOffset());
  write(`2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n`);

  xref.push(currentOffset());
  write(`3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 ${width} ${height}] /Contents 4 0 R /Resources << /XObject << /Img 5 0 R >> >> >>\nendobj\n`);

  const contentStr = `q ${width} 0 0 ${height} 0 0 cm /Img Do Q`;
  const contentBytes = enc.encode(contentStr);
  xref.push(currentOffset());
  write(`4 0 obj\n<< /Length ${contentBytes.length} >>\nstream\n`);
  writeBytes(contentBytes);
  write("\nendstream\nendobj\n");

  xref.push(currentOffset());
  write(`5 0 obj\n<< /Type /XObject /Subtype /Image /Width ${width} /Height ${height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length ${jpegBytes.length} >>\nstream\n`);
  writeBytes(jpegBytes);
  write("\nendstream\nendobj\n");

  const xrefOffset = currentOffset();
  write("xref\n");
  write(`0 ${xref.length + 1}\n`);
  write("0000000000 65535 f \n");
  for (const offset of xref) {
    write(String(offset).padStart(10, "0") + " 00000 n \n");
  }

  write("trailer\n");
  write(`<< /Size ${xref.length + 1} /Root 1 0 R >>\n`);
  write("startxref\n");
  write(`${xrefOffset}\n`);
  write("%%EOF\n");

  let totalLen = 0;
  for (const p of parts) totalLen += p.length;
  const result = new Uint8Array(totalLen);
  let pos = 0;
  for (const p of parts) {
    result.set(p, pos);
    pos += p.length;
  }

  return result;
}

// ---- Public API (called from C# via JS interop) ----

export async function renderToPngBytes(snapshotJson) {
  const snapshot = parseSnapshot(snapshotJson);
  const svgStr = buildSvg(snapshot);
  const scale = snapshot.options?.scale ?? 1;
  const w = Math.ceil(snapshot.canvasWidth * scale);
  const h = Math.ceil(snapshot.canvasHeight * scale);
  const canvas = await svgToCanvas(svgStr, w, h);
  return await canvasToBytes(canvas);
}

export async function exportAsPng(snapshotJson, fileName) {
  const snapshot = parseSnapshot(snapshotJson);
  const svgStr = buildSvg(snapshot);
  const scale = snapshot.options?.scale ?? 1;
  const w = Math.ceil(snapshot.canvasWidth * scale);
  const h = Math.ceil(snapshot.canvasHeight * scale);
  const canvas = await svgToCanvas(svgStr, w, h);
  const bytes = await canvasToBytes(canvas);
  triggerDownload(bytes, `${fileName}.png`, "image/png");
}

export async function exportAsPdf(snapshotJson, fileName) {
  const snapshot = parseSnapshot(snapshotJson);
  const pdfBytes = await buildMinimalPdf(snapshot);
  triggerDownload(pdfBytes, `${fileName}.pdf`, "application/pdf");
}

export function exportAsSvg(snapshotJson, fileName) {
  const snapshot = parseSnapshot(snapshotJson);
  const svgStr = buildSvg(snapshot);
  const bytes = new TextEncoder().encode(svgStr);
  triggerDownload(bytes, `${fileName}.svg`, "image/svg+xml");
}
