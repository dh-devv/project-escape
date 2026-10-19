(() => {
  'use strict';

  const sheets = window.LAST_SPARK_SPRITES || {};
  const images = new Map();
  const alphaBounds = new WeakMap();
  let scanCanvas = null;
  let scanContext = null;

  function imageFor(name, sheet) {
    let image = images.get(name);
    if (!image) {
      image = new Image();
      image.src = sheet.src;
      images.set(name, image);
    }
    return image;
  }

  function size(name, options = {}) {
    const sheet = sheets[name];
    if (!sheet) return null;
    const baseScale = Number.isFinite(options.baseScale) ? Math.max(0, options.baseScale) : sheet.scale;
    const scale = baseScale * (Number.isFinite(options.scale) ? Math.max(0, options.scale) : 1);
    return { width: sheet.width * scale, height: sheet.height * scale };
  }

  function visibleBounds(image, sheet, frame) {
    let cache = alphaBounds.get(image);
    if (!cache) {
      cache = new Map();
      alphaBounds.set(image, cache);
    }
    if (cache.has(frame)) return cache.get(frame);
    if (typeof document === 'undefined') return null;
    if (!scanCanvas) {
      scanCanvas = document.createElement('canvas');
      scanContext = scanCanvas.getContext('2d', { willReadFrequently: true });
    }
    if (!scanContext) return null;
    scanCanvas.width = sheet.width;
    scanCanvas.height = sheet.height;
    const sourceX = (frame % sheet.columns) * sheet.width;
    const sourceY = Math.floor(frame / sheet.columns) * sheet.height;
    try {
      scanContext.clearRect(0, 0, sheet.width, sheet.height);
      scanContext.drawImage(image, sourceX, sourceY, sheet.width, sheet.height, 0, 0, sheet.width, sheet.height);
      const pixels = scanContext.getImageData(0, 0, sheet.width, sheet.height).data;
      let left = sheet.width, top = sheet.height, right = -1, bottom = -1;
      for (let y = 0; y < sheet.height; y++) {
        for (let x = 0; x < sheet.width; x++) {
          if (pixels[(y * sheet.width + x) * 4 + 3] <= 12) continue;
          if (x < left) left = x;
          if (x > right) right = x;
          if (y < top) top = y;
          if (y > bottom) bottom = y;
        }
      }
      const bounds = right < left || bottom < top ? null : { x: left, y: top, width: right - left + 1, height: bottom - top + 1 };
      cache.set(frame, bounds);
      return bounds;
    } catch (_) {
      cache.set(frame, null);
      return null;
    }
  }

  function draw(ctx, name, elapsed, x, y, options = {}) {
    let drawName = name;
    let sheet = sheets[drawName];
    let image = sheet ? imageFor(drawName, sheet) : null;
    if (!image || !image.complete || image.naturalWidth === 0) {
      drawName = options.fallback;
      sheet = drawName && sheets[drawName];
      if (!sheet) return false;
      image = imageFor(drawName, sheet);
      if (!image.complete || image.naturalWidth === 0) return false;
    }

    const frameCount = Math.max(1, sheet.frames);
    const frame = Number.isFinite(options.frame)
      ? Math.max(0, Math.min(frameCount - 1, Math.floor(options.frame)))
      : sheet.loop
        ? Math.floor(Math.max(0, elapsed) * sheet.fps) % frameCount
        : Math.min(frameCount - 1, Math.floor(Math.max(0, elapsed) * sheet.fps));
    let sourceX = (frame % sheet.columns) * sheet.width;
    let sourceY = Math.floor(frame / sheet.columns) * sheet.height;
    const dimensions = size(drawName, options);
    let sourceWidth = sheet.width;
    let sourceHeight = sheet.height;
    let width = dimensions.width;
    let height = dimensions.height;
    let left = x - width * sheet.pivot[0];
    let top = y - height * (1 - sheet.pivot[1]);
    if (Number.isFinite(options.fitHeight) && options.fitHeight > 0) {
      const bounds = visibleBounds(image, sheet, frame);
      const fitted = bounds && window.LastSparkPresentation && window.LastSparkPresentation.fitToHeight(bounds, options.fitHeight);
      if (fitted) {
        sourceX += bounds.x;
        sourceY += bounds.y;
        sourceWidth = bounds.width;
        sourceHeight = bounds.height;
        width = fitted.width;
        height = fitted.height;
        left = x - width / 2;
        top = y - height;
      } else {
        height = options.fitHeight;
        width = height * sheet.width / sheet.height;
        left = x - width / 2;
        top = y - height;
      }
    }

    ctx.save();
    ctx.globalAlpha *= options.alpha == null ? 1 : options.alpha;
    if (options.flip) {
      ctx.translate(x * 2, 0);
      ctx.scale(-1, 1);
    }
    ctx.drawImage(image, sourceX, sourceY, sourceWidth, sourceHeight, left, top, width, height);
    ctx.restore();
    return true;
  }

  window.LastSparkAssets = Object.freeze({ draw, size, has: name => Boolean(sheets[name]) });
})();
