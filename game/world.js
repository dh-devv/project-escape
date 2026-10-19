(() => {
  'use strict';

  function create(roomWidths, generators = []) {
    const widths = Array.isArray(roomWidths) && roomWidths.length
      ? roomWidths.map(width => Math.max(1, Number(width) || 960))
      : [960, 960, 960, 960];
    const seen = new Set();
    const devices = (Array.isArray(generators) ? generators : []).filter(item => {
      if (!item || item.id == null || seen.has(String(item.id))) return false;
      seen.add(String(item.id));
      return true;
    }).map(item => ({
      id: String(item.id),
      room: Math.max(0, Math.floor(Number(item.room) || 0)),
      floor: Math.max(1, Math.min(3, Math.floor(Number(item.floor) || 1))),
      x: Math.max(0, Number(item.x) || 0),
      active: Boolean(item.active),
    }));

    return {
      roomWidths: widths,
      generators: devices,
      powerLevel: Math.min(3, devices.filter(device => device.active).length),
    };
  }

  function roomWidth(world, roomIndex) {
    return world && world.roomWidths[roomIndex] || 960;
  }

  function cameraX(world, roomIndex, playerX, viewportWidth) {
    const room = roomWidth(world, roomIndex);
    const viewport = Math.max(1, Number(viewportWidth) || 960);
    const max = Math.max(0, room - viewport);
    return Math.max(0, Math.min(max, (Number(playerX) || 0) - viewport / 2));
  }

  function activateGenerator(world, id) {
    if (!world || !Array.isArray(world.generators)) return false;
    const generator = world.generators.find(item => item.id === String(id));
    if (!generator || generator.active) return false;
    generator.active = true;
    world.powerLevel = Math.min(3, world.powerLevel + 1);
    return true;
  }

  function generatorsInRoom(world, roomIndex, floorIndex) {
    return world && Array.isArray(world.generators)
      ? world.generators.filter(generator => generator.room === roomIndex && (floorIndex == null || generator.floor === floorIndex))
      : [];
  }

  window.LastSparkWorld = Object.freeze({ create, roomWidth, cameraX, activateGenerator, generatorsInRoom });
})();
