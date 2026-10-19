(() => {
  'use strict';

  const clamp = (value, min, max) => Math.max(min, Math.min(max, value));

  function audioLevels(powerLevel) {
    const power = clamp(Math.floor(Number(powerLevel) || 0), 0, 3);
    const progress = power / 3;
    return Object.freeze({
      noise: 0.012 * Math.pow(0.16, progress),
      drone: 0.018 * Math.pow(0.12, progress)
    });
  }

  function fitToHeight(bounds, targetHeight) {
    if (!bounds || !(bounds.width > 0) || !(bounds.height > 0) || !(targetHeight > 0)) return null;
    return {
      x: bounds.x,
      y: bounds.y,
      width: bounds.width * targetHeight / bounds.height,
      height: targetHeight
    };
  }

  function alarmPeriod(powerLevel) {
    const power = clamp(Math.floor(Number(powerLevel) || 0), 0, 3);
    return [2, 1.7, 1.35, 1.05][power];
  }

  function emergencyPulse(time, powerLevel = 0) {
    const period = alarmPeriod(powerLevel);
    return clamp(0.175 + 0.175 * Math.sin((Number(time) || 0) * Math.PI * 2 / period), 0, 0.35);
  }

  function interactionBob(time) {
    return Math.sin((Number(time) || 0) * 4.5) * 1.5;
  }

  const api = Object.freeze({ audioLevels, fitToHeight, emergencyPulse, alarmPeriod, interactionBob });
  if (typeof window !== 'undefined') window.LastSparkPresentation = api;
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
})();
