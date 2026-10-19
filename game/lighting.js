(() => {
  'use strict';

  const clamp = (value, min, max) => Math.max(min, Math.min(max, value));
  let darknessCanvas = null;
  let darknessContext = null;

  function alpha(powerLevel, pulse) {
    const power = clamp(Math.floor(Number(powerLevel) || 0), 0, 3);
    return [1, 0.6, 0.32, 0.02][power];
  }

  function flashlightRadius(powerLevel) {
    const power = clamp(Math.floor(Number(powerLevel) || 0), 0, 3);
    return [145, 210, 280, 145][power];
  }

  function glitchIntensity(powerLevel) {
    return 0;
  }

  function flickerAmplitude(powerLevel) {
    return 0;
  }

  function sparkIntensity(powerLevel) {
    const power = clamp(Math.floor(Number(powerLevel) || 0), 0, 3);
    return 0.28 + (1 - power / 3) * 0.72;
  }

  function pulse(time, seed = 0) {
    const t = Number(time) || 0;
    const s = Number(seed) || 0;
    return 0.45 * Math.sin(t * 11.3 + s) +
      0.35 * Math.sin(t * 24.7 + s * 1.7) +
      0.2 * Math.sin(t * 49.1 + s * 2.1);
  }

  function drawFlashlight(ctx, flashlight) {
    if (!flashlight || !(flashlight.radius > 0)) return;
    const x = Number(flashlight.x) || 0;
    const y = Number(flashlight.y) || 0;
    const angle = flashlight.face < 0 ? Math.PI : 0;
    const radius = clamp(flashlight.radius, 80, 420);
    const center = Math.atan2(Number(flashlight.directionY) || 0, Number(flashlight.directionX) || flashlight.face || 1);
    const aim = Number.isFinite(flashlight.directionX) || Number.isFinite(flashlight.directionY) ? center : angle;
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    for (let layer = 2; layer >= 0; layer--) {
      const halfAngle = 0.27 + layer * 0.065;
      const beam = ctx.createRadialGradient(x, y, 12, x + Math.cos(aim) * radius * 0.58, y + Math.sin(aim) * radius * 0.58, radius);
      beam.addColorStop(0, `rgba(166, 198, 168, ${0.045 - layer * 0.009})`);
      beam.addColorStop(0.62, `rgba(142, 181, 156, ${0.035 - layer * 0.007})`);
      beam.addColorStop(1, 'rgba(110, 154, 137, 0)');
      ctx.fillStyle = beam;
      ctx.beginPath();
      ctx.moveTo(x, y);
      ctx.arc(x, y, radius, aim - halfAngle, aim + halfAngle);
      ctx.closePath();
      ctx.fill();
    }
    ctx.restore();
  }

  function drawAlarm(ctx, width, height, amount) {
    if (!(amount > 0)) return;
    ctx.save();
    ctx.globalCompositeOperation = 'source-over';
    ctx.fillStyle = `rgba(108, 24, 22, ${clamp(amount, 0, 0.35)})`;
    ctx.fillRect(0, 0, width, height);
    ctx.globalCompositeOperation = 'lighter';
    for (const x of [0, width]) {
      const beacon = ctx.createRadialGradient(x, 0, 4, x, 0, Math.min(width, height) * 0.62);
      beacon.addColorStop(0, `rgba(194, 50, 38, ${clamp(amount, 0, 0.35) * 0.72})`);
      beacon.addColorStop(1, 'rgba(130, 28, 26, 0)');
      ctx.fillStyle = beacon;
      ctx.fillRect(0, 0, width, height);
    }
    ctx.restore();
  }

  function getDarknessLayer(width, height) {
    if (!darknessCanvas) {
      if (typeof OffscreenCanvas !== 'undefined') darknessCanvas = new OffscreenCanvas(width, height);
      else if (typeof document !== 'undefined') darknessCanvas = document.createElement('canvas');
      if (darknessCanvas) darknessContext = darknessCanvas.getContext('2d');
    }
    if (!darknessCanvas || !darknessContext) return null;
    if (darknessCanvas.width !== width) darknessCanvas.width = width;
    if (darknessCanvas.height !== height) darknessCanvas.height = height;
    return darknessContext;
  }

  function cutLightCircle(ctx, source, strength) {
    if (!source || !(source.radius > 0) || !(strength > 0)) return;
    const x = Number(source.x) || 0;
    const y = Number(source.y) || 0;
    const radius = clamp(source.radius, 1, 800);
    const amount = clamp(strength, 0, 1);
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
    gradient.addColorStop(0, `rgba(0, 0, 0, ${amount})`);
    gradient.addColorStop(0.72, `rgba(0, 0, 0, ${amount * 0.78})`);
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, Math.PI * 2);
    ctx.fill();
  }

  function cutFlashlightCone(ctx, flashlight) {
    if (!flashlight || !(flashlight.radius > 0)) return;
    const x = Number(flashlight.x) || 0;
    const y = Number(flashlight.y) || 0;
    const angle = flashlight.face < 0 ? Math.PI : 0;
    const radius = clamp(flashlight.radius, 80, 420);
    const center = Math.atan2(Number(flashlight.directionY) || 0, Number(flashlight.directionX) || flashlight.face || 1);
    const aim = Number.isFinite(flashlight.directionX) || Number.isFinite(flashlight.directionY) ? center : angle;
    const gradient = ctx.createRadialGradient(x, y, 10, x + Math.cos(aim) * radius * 0.58, y + Math.sin(aim) * radius * 0.58, radius);
    gradient.addColorStop(0, 'rgba(0, 0, 0, 0.98)');
    gradient.addColorStop(0.65, 'rgba(0, 0, 0, 0.88)');
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.arc(x, y, radius, aim - 0.34, aim + 0.34);
    ctx.closePath();
    ctx.fill();
  }

  function drawAmbientLights(ctx, lightSources) {
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    for (const source of lightSources) {
      if (!source || !(source.radius > 0) || !(source.intensity > 0)) continue;
      const glow = ctx.createRadialGradient(source.x, source.y, 0, source.x, source.y, source.radius);
      glow.addColorStop(0, `rgba(120, 205, 194, ${clamp(source.intensity, 0, 0.5)})`);
      glow.addColorStop(1, 'rgba(70, 140, 145, 0)');
      ctx.fillStyle = glow;
      ctx.beginPath();
      ctx.arc(source.x, source.y, source.radius, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.restore();
  }

  function draw(ctx, width, height, darkness, lightSources = [], flashlight = null, alarm = 0, restorationFlash = 0) {
    if (!ctx || !Number.isFinite(width) || !Number.isFinite(height)) return;
    const layer = getDarknessLayer(width, height);
    if (layer) {
      layer.save();
      layer.clearRect(0, 0, width, height);
      layer.globalCompositeOperation = 'source-over';
      layer.globalAlpha = clamp(Number(darkness) || 0, 0, 1);
      layer.fillStyle = '#020509';
      layer.fillRect(0, 0, width, height);
      layer.globalAlpha = 1;
      if (darkness <= 0.025) {
        layer.fillStyle = 'rgba(190, 196, 181, 0.28)';
        layer.fillRect(0, 0, width, height);
      }
      if (restorationFlash > 0) {
        layer.fillStyle = `rgba(228, 235, 214, ${clamp(restorationFlash, 0, 1) * 0.34})`;
        layer.fillRect(0, 0, width, height);
      }
      layer.globalCompositeOperation = 'destination-out';
      for (const source of lightSources) cutLightCircle(layer, source, Math.min(0.88, source.intensity * 1.8));
      cutFlashlightCone(layer, flashlight);
      layer.restore();
      ctx.save();
      ctx.globalCompositeOperation = 'source-over';
      ctx.globalAlpha = 1;
      ctx.drawImage(darknessCanvas, 0, 0);
      ctx.restore();
    } else {
      // Fallback for canvases without an offscreen layer: retain a visible, softened beam.
      ctx.save();
      ctx.globalCompositeOperation = 'source-over';
      ctx.globalAlpha = clamp(Number(darkness) || 0, 0, 1);
      ctx.fillStyle = '#020509';
      ctx.fillRect(0, 0, width, height);
      ctx.restore();
    }
    drawAmbientLights(ctx, lightSources);
    drawFlashlight(ctx, flashlight);
    drawAlarm(ctx, width, height, alarm);
  }

  function drawGlitch(ctx, width, height, intensity, time) {
    // Interference belongs at broken electrical fixtures, not across the whole view.
  }

  window.LastSparkLighting = Object.freeze({ alpha, flashlightRadius, pulse, glitchIntensity, flickerAmplitude, sparkIntensity, draw, drawGlitch });
})();
