(function (root) {
  'use strict';

  function playerBounds(player) {
    return { left: player.x - 12, right: player.x + 12, top: player.y - 67, bottom: player.y };
  }

  function circleHitsPlayer(circle, player) {
    const box = playerBounds(player);
    const x = Math.max(box.left, Math.min(circle.x, box.right));
    const y = Math.max(box.top, Math.min(circle.y, box.bottom));
    const radius = Math.max(0, Number(circle.radius) || 0);
    return (circle.x - x) ** 2 + (circle.y - y) ** 2 <= radius ** 2;
  }

  function circleHitsRect(circle, rect) {
    if (!circle || !rect || !Number.isFinite(circle.x) || !Number.isFinite(circle.y) ||
        !Number.isFinite(rect.x) || !Number.isFinite(rect.y) ||
        !Number.isFinite(rect.width) || !Number.isFinite(rect.height)) return false;
    const left = Math.min(rect.x, rect.x + rect.width);
    const right = Math.max(rect.x, rect.x + rect.width);
    const top = Math.min(rect.y, rect.y + rect.height);
    const bottom = Math.max(rect.y, rect.y + rect.height);
    const x = Math.max(left, Math.min(circle.x, right));
    const y = Math.max(top, Math.min(circle.y, bottom));
    const radius = Math.max(0, Number(circle.radius) || 0);
    return (circle.x - x) ** 2 + (circle.y - y) ** 2 <= radius ** 2;
  }

  function playerOverlapsRect(player, rect) {
    const box = playerBounds(player);
    return box.left <= rect.x + rect.width && box.right >= rect.x &&
      box.top <= rect.y + rect.height && box.bottom >= rect.y;
  }

  function meleeHitsTarget(attacker, target, facing, reach) {
    if (!attacker || !target || !Number.isFinite(attacker.x) || !Number.isFinite(attacker.y) ||
        !Number.isFinite(target.x) || !Number.isFinite(target.y)) return false;
    const direction = Number(facing) < 0 ? -1 : 1;
    const range = Math.max(12, Number(reach) || 0);
    const attackerHeight = Math.max(1, Number(attacker.height) || 67);
    const targetWidth = Math.max(1, Number(target.width) || 48);
    const targetHeight = Math.max(1, Number(target.height) || 80);
    const hitLeft = direction > 0 ? attacker.x + 8 : attacker.x - range;
    const hitRight = direction > 0 ? attacker.x + range : attacker.x - 8;
    const targetLeft = target.x - targetWidth / 2;
    const targetRight = target.x + targetWidth / 2;
    const hitTop = attacker.y - attackerHeight + 5;
    const hitBottom = attacker.y - 8;
    const targetTop = target.y - targetHeight;
    return hitLeft <= targetRight && hitRight >= targetLeft &&
      hitTop <= target.y && hitBottom >= targetTop;
  }

  function hazardHitsPlayer(hazard, player, boss, floor, width) {
    if (hazard.kind === 'rush') {
      return playerOverlapsRect(player, { x: boss.x - boss.w * 0.42, y: floor - boss.h * 0.72, width: boss.w * 0.84, height: boss.h * 0.72 });
    }
    if (hazard.kind === 'lane') {
      return playerOverlapsRect(player, { x: hazard.x1, y: floor - 7, width: hazard.x2 - hazard.x1, height: 34 });
    }
    if (hazard.kind === 'ring') {
      const centerX = Number.isFinite(hazard.x) ? hazard.x : boss.x;
      return playerOverlapsRect(player, { x: centerX - hazard.radius, y: floor - 35, width: hazard.radius * 2, height: 60 });
    }
    if (hazard.kind === 'split') {
      const warning = { y: floor - 8, height: 36 };
      return playerOverlapsRect(player, { x: 0, ...warning, width: hazard.safe1 }) ||
        playerOverlapsRect(player, { x: hazard.safe2, ...warning, width: width - hazard.safe2 });
    }
    return false;
  }

  const CHARGE_THRESHOLD_SECONDS = 0.65;
  const CHARGE_MOVE_MULTIPLIER = 0.6;
  const DASH_COOLDOWN_SECONDS = 5;
  const PLAYER_PHASE_PROFILES = Object.freeze([
    Object.freeze({ speed: 150, damageTakenMultiplier: 0.45 }),
    Object.freeze({ speed: 210, damageTakenMultiplier: 0.75 }),
    Object.freeze({ speed: 280, damageTakenMultiplier: 1.1 })
  ]);

  function playerPhaseProfile(phase) {
    const index = Math.max(0, Math.min(2, Math.round(Number(phase) || 1) - 1));
    return PLAYER_PHASE_PROFILES[index];
  }

  function attackProfile(chargeSeconds) {
    var charged = Number(chargeSeconds) >= CHARGE_THRESHOLD_SECONDS;
    return charged ? {
      charged: true,
      startup: 0.18,
      active: 0.14,
      recovery: 0.42,
      cooldown: 0.74,
      damageMultiplier: 2.4,
      moveMultiplier: 0.6
    } : {
      charged: false,
      startup: 0.1,
      active: 0.1,
      recovery: 0.28,
      cooldown: 0.48,
      damageMultiplier: 1,
      moveMultiplier: 1
    };
  }

  function usePotion(player) {
    if (!player || !Number.isFinite(player.hp) || !Number.isFinite(player.max) ||
        player.hp <= 0 || player.alive === false || player.hp >= player.max ||
        !Number.isFinite(player.potions) || player.potions < 1) return 0;
    var before = player.hp;
    player.hp = Math.min(player.max, player.hp + player.max * 0.1);
    player.potions -= 1;
    return player.hp - before;
  }

  function chargeMoveMultiplier(chargeSeconds) {
    return Number(chargeSeconds) > 0 ? CHARGE_MOVE_MULTIPLIER : 1;
  }

  const api = Object.freeze({
    playerBounds, circleHitsPlayer, circleHitsRect, playerOverlapsRect, meleeHitsTarget, hazardHitsPlayer,
    CHARGE_THRESHOLD_SECONDS, CHARGE_MOVE_MULTIPLIER, DASH_COOLDOWN_SECONDS, attackProfile, chargeMoveMultiplier,
    playerPhaseProfile, usePotion
  });
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else root.LastSparkCombat = api;
})(typeof globalThis !== 'undefined' ? globalThis : window);
