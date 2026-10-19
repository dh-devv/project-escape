(() => {
  'use strict';

  const cues = Object.freeze({
    uiClick: Object.freeze({ type: 'sawtooth', frequency: 220, endFrequency: 85, gain: 0.006, duration: 0.038, noise: Object.freeze({ gain: 0.012, duration: 0.027, frequency: 1400 }) }),
    swing: Object.freeze({ type: 'sawtooth', frequency: 220, endFrequency: 65, gain: 0.009, duration: 0.085, noise: Object.freeze({ gain: 0.018, duration: 0.075, frequency: 1250 }) }),
    chargeReady: Object.freeze({ type: 'sawtooth', frequency: 110, endFrequency: 48, gain: 0.016, duration: 0.12, noise: Object.freeze({ gain: 0.01, duration: 0.07, frequency: 480 }) }),
    charged: Object.freeze({ type: 'sawtooth', frequency: 130, endFrequency: 48, gain: 0.018, duration: 0.11, noise: Object.freeze({ gain: 0.014, duration: 0.065, frequency: 560 }) }),
    impact: Object.freeze({ type: 'triangle', frequency: 125, endFrequency: 38, gain: 0.085, duration: 0.15, noise: Object.freeze({ gain: 0.028, duration: 0.055, frequency: 850 }) }),
    heavyImpact: Object.freeze({ type: 'triangle', frequency: 135, endFrequency: 32, gain: 0.12, duration: 0.19, noise: Object.freeze({ gain: 0.042, duration: 0.075, frequency: 700 }) }),
    hurtSuit: Object.freeze({ type: 'sawtooth', frequency: 185, endFrequency: 58, gain: 0.038, duration: 0.095, notes: Object.freeze([
      Object.freeze({ offset: 0, frequency: 185, endFrequency: 58, duration: 0.095, attack: 0.002 }),
      Object.freeze({ offset: 0.008, type: 'sawtooth', frequency: 290, endFrequency: 95, gain: 0.007, duration: 0.048, attack: 0.002 })
    ]), noise: Object.freeze([
      Object.freeze({ gain: 0.043, duration: 0.06, frequency: 2300 }),
      Object.freeze({ gain: 0.018, duration: 0.105, frequency: 580 })
    ]) }),
    hurtExposed: Object.freeze({ type: 'triangle', frequency: 165, endFrequency: 65, gain: 0.061, duration: 0.12, notes: Object.freeze([
      Object.freeze({ offset: 0, frequency: 165, endFrequency: 65, duration: 0.12, attack: 0.006 }),
      Object.freeze({ offset: 0.008, type: 'sawtooth', frequency: 280, endFrequency: 90, gain: 0.009, duration: 0.052, attack: 0.002 })
    ]), noise: Object.freeze({ gain: 0.028, duration: 0.075, frequency: 850 }) }),
    hurtHuman: Object.freeze({ type: 'triangle', frequency: 155, endFrequency: 60, gain: 0.075, duration: 0.115, noise: Object.freeze([
      Object.freeze({ offset: 0, gain: 0.033, duration: 0.075, frequency: 340 }),
      Object.freeze({ offset: 0.045, gain: 0.018, duration: 0.18, frequency: 1150 })
    ]) }),
    inject: Object.freeze({ type: 'triangle', frequency: 190, endFrequency: 75, gain: 0.012, duration: 0.18 }),
    alarm: Object.freeze({ type: 'sine', frequency: 72, endFrequency: 48, gain: 0.032, duration: 0.42, spread: 0.025 }),
    wingSiren: Object.freeze({ type: 'triangle', frequency: 520, endFrequency: 410, gain: 0.027, duration: 0.7, spread: 0.012, notes: Object.freeze([
      Object.freeze({ offset: 0, frequency: 520, endFrequency: 660, duration: 0.3 }),
      Object.freeze({ offset: 0.35, frequency: 660, endFrequency: 410, duration: 0.3 })
    ]) }),
    motor: Object.freeze({ type: 'sine', frequency: 68, endFrequency: 54, gain: 0.012, duration: 0.34, spread: 0.04 }),
    powerRestore: Object.freeze({ type: 'sawtooth', frequency: 95, endFrequency: 260, gain: 0.045, duration: 0.32, spread: 0.02 })
  });

  function cue(name) {
    return cues[name] || null;
  }

  function pitchMultiplier(random = Math.random, spread = 0.06) {
    const sample = Math.max(0, Math.min(1, Number(random()) || 0));
    return 1 - spread + sample * spread * 2;
  }

  function play(audioContext, name, random = Math.random) {
    const sound = cue(name);
    if (!audioContext || !sound) return false;
    try {
      const pitch = pitchMultiplier(random, sound.spread ?? 0.06);
      const notes = sound.notes || [{ offset: 0, frequency: sound.frequency, endFrequency: sound.endFrequency, duration: sound.duration }];
      for (const note of notes) {
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();
        const now = audioContext.currentTime + note.offset;
        oscillator.type = note.type || sound.type;
        oscillator.frequency.setValueAtTime(note.frequency * pitch, now);
        oscillator.frequency.exponentialRampToValueAtTime(note.endFrequency * pitch, now + note.duration);
        gainNode.gain.setValueAtTime(0.0001, now);
        gainNode.gain.exponentialRampToValueAtTime(note.gain ?? sound.gain, now + (note.attack ?? 0.012));
        gainNode.gain.exponentialRampToValueAtTime(0.0001, now + note.duration);
        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);
        oscillator.start(now);
        oscillator.stop(now + note.duration + 0.015);
      }
      for (const noise of sound.noise ? (Array.isArray(sound.noise) ? sound.noise : [sound.noise]) : []) {
        const start = audioContext.currentTime + (noise.offset || 0);
        const length = Math.ceil(audioContext.sampleRate * noise.duration);
        const buffer = audioContext.createBuffer(1, length, audioContext.sampleRate);
        const samples = buffer.getChannelData(0);
        for (let i = 0; i < length; i++) samples[i] = random() * 2 - 1;
        const source = audioContext.createBufferSource();
        source.buffer = buffer;
        const filter = audioContext.createBiquadFilter();
        filter.type = 'bandpass';
        filter.frequency.value = noise.frequency;
        filter.Q.value = 0.7;
        const noiseGain = audioContext.createGain();
        noiseGain.gain.setValueAtTime(0.0001, start);
        noiseGain.gain.exponentialRampToValueAtTime(noise.gain, start + 0.004);
        noiseGain.gain.exponentialRampToValueAtTime(0.0001, start + noise.duration);
        source.connect(filter).connect(noiseGain).connect(audioContext.destination);
        source.start(start);
        source.stop(start + noise.duration + 0.01);
      }
      return true;
    } catch (_) {
      return false;
    }
  }

  const api = Object.freeze({ cue, pitchMultiplier, play });
  if (typeof window !== 'undefined') window.LastSparkSound = api;
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
})();
