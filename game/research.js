(function (root) {
  'use strict';

  var FLOOR_DATA = [
    { number: 1, name: '정비·비상전력실', generatorId: 'g1', coverXs: [500], patrols: [{ x: 680, kind: 'ranged' }] },
    { number: 2, name: '격리 관찰동', generatorId: 'g2', coverXs: [830], patrols: [{ x: 660, kind: 'melee' }] },
    { number: 3, name: '중앙 배전실', generatorId: 'g3', coverXs: [430], patrols: [{ x: 720, kind: 'charger' }] }
  ];

  function floors() {
    return FLOOR_DATA.map(function (floor) {
      return { number: floor.number, name: floor.name, generatorId: floor.generatorId };
    });
  }

  function floor(number) {
    var index = Math.floor(Number(number)) - 1;
    return FLOOR_DATA[index] || FLOOR_DATA[0];
  }

  function generatorFloor(id) {
    for (var i = 0; i < FLOOR_DATA.length; i += 1) {
      if (FLOOR_DATA[i].generatorId === String(id)) return FLOOR_DATA[i].number;
    }
    return null;
  }

  function dropWave(floorNumber, roomWidth, waveNumber) {
    var width = Math.max(320, Number(roomWidth) || 1200);
    var floorId = Math.max(1, Math.min(3, Math.floor(Number(floorNumber) || 1)));
    var wave = Math.max(0, Math.floor(Number(waveNumber) || 0));
    var kinds = ['melee', 'ranged', 'charger'];
    return [0, 1, 2].map(function (index) {
      var ratio = [0.29, 0.53, 0.77][index];
      return {
        x: Math.round(width * ratio),
        kind: kinds[(floorId + wave + index) % kinds.length]
      };
    });
  }

  function targetedSafeGap(playerX, roomWidth, gapWidth) {
    var width = Math.max(320, Number(roomWidth) || 960);
    var gap = Math.max(80, Math.min(width * 0.38, Number(gapWidth) || 148));
    var target = Math.max(0, Math.min(width, Number(playerX) || width / 2));
    var center = target < width / 2 ? target + gap * 1.35 : target - gap * 1.35;
    center = Math.max(gap / 2, Math.min(width - gap / 2, center));
    return { safe1: Math.round(center - gap / 2), safe2: Math.round(center + gap / 2) };
  }

  function hide(player, cover) {
    if (!player || !cover || Math.abs(Number(player.x) - Number(cover.x)) > 64) return false;
    player.hidden = true;
    player.coverId = String(cover.id);
    player.vx = 0;
    player.vy = 0;
    return true;
  }

  function reveal(player) {
    if (!player || !player.hidden) return false;
    player.hidden = false;
    player.coverId = null;
    return true;
  }

  function wakeNearby(enemies, x, radius, now) {
    var woken = 0;
    (Array.isArray(enemies) ? enemies : []).forEach(function (enemy) {
      if (!enemy || Math.abs(enemy.x - x) > radius || enemy.hp <= 0) return;
      if (enemy.alerted) return;
      enemy.noticeUntil = (Number(now) || 0) + .85;
      enemy.noticeAlarm = true;
      enemy.lastSeenX = x;
      woken += 1;
    });
    return woken;
  }

  function stepPatrol(enemy, dt, player, now) {
    if (!enemy || !isFinite(dt) || dt <= 0) return;

    var min = Number(enemy.patrolMin);
    var max = Number(enemy.patrolMax);
    if (!isFinite(min)) min = 36;
    if (!isFinite(max) || max < min) max = 1164;
    var direction = enemy.facing < 0 ? -1 : 1;
    var target = enemy.alerted ? Number(enemy.lastSeenX) : NaN;
    if (enemy.alerted && isFinite(target)) {
      direction = target < enemy.x ? -1 : 1;
      var distance = Math.abs(target - enemy.x);
      if (distance <= 8 && (Number(now) || 0) >= (Number(enemy.searchUntil) || 0)) {
        enemy.alerted = false;
        direction = enemy.patrolHomeFacing < 0 ? -1 : 1;
      } else {
        enemy.facing = direction;
        enemy.x += direction * Math.min(distance, (Number(enemy.speed) || 36) * 1.3 * dt);
        enemy.vx = direction * (Number(enemy.speed) || 36) * 1.3;
        return;
      }
    }

    enemy.facing = direction;
    enemy.x += direction * (Number(enemy.speed) || 36) * dt;
    if (enemy.x <= min) {
      enemy.x = min;
      enemy.facing = 1;
      enemy.patrolHomeFacing = 1;
    } else if (enemy.x >= max) {
      enemy.x = max;
      enemy.facing = -1;
      enemy.patrolHomeFacing = -1;
    }
    enemy.vx = enemy.facing * (Number(enemy.speed) || 36);
  }

  root.LastSparkResearch = {
    floors: floors,
    floor: floor,
    generatorFloor: generatorFloor,
    dropWave: dropWave,
    targetedSafeGap: targetedSafeGap,
    hide: hide,
    reveal: reveal,
    wakeNearby: wakeNearby,
    stepPatrol: stepPatrol
  };
})(typeof window !== 'undefined' ? window : this);
