(() => {
  'use strict';

  const SAVE_KEY = 'last-spark.local-profile.v1';
  const ID_KEY = 'last-spark.profile-id.v1';
  const API_BASE = String(window.LAST_SPARK_API_BASE || '/api/v1').replace(/\/$/, '');
  const defaults = { runs: 0, playSeconds: 0, bestRoom: 0, bestPhase: 0, bestClearSeconds: null };
  let stats = { ...defaults };
  let profileId = '';
  let syncLabel = location.protocol === 'file:' ? 'DB 미연결 · 주소창 실행' : '이 기기 기록';
  try {
    const saved = JSON.parse(localStorage.getItem(SAVE_KEY) || '{}');
    stats = { ...defaults, ...saved };
    profileId = localStorage.getItem(ID_KEY) || '';
  } catch { /* Private browsing can disable local storage. */ }

  const makeId = () => {
    if (window.crypto && typeof window.crypto.randomUUID === 'function') return window.crypto.randomUUID();
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, char => {
      const random = Math.random() * 16 | 0;
      return (char === 'x' ? random : (random & 3 | 8)).toString(16);
    });
  };
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(profileId)) {
    profileId = makeId();
    try { localStorage.setItem(ID_KEY, profileId); } catch { /* Sync can still use the in-memory id. */ }
  }

  const style = document.createElement('style');
  style.textContent = `
    #ls-intro,#ls-records{position:fixed;inset:0;z-index:99999;display:grid;place-items:center;padding:18px;background:radial-gradient(ellipse at 50% 48%,#17201c 0,#080c0b 62%,#030505 100%);color:#c8c9ba;font:14px/1.55 "Malgun Gothic",Arial,sans-serif}
    #ls-intro[hidden],#ls-records[hidden]{display:none}
    .ls-page{width:min(920px,100%);max-height:94vh;overflow:auto;padding:clamp(18px,4vw,38px);background:linear-gradient(145deg,#171d19,#101512 72%,#151a15);color:#c8c9ba;border:1px solid #475044;box-shadow:12px 14px 0 #000,0 0 0 5px #101411;position:relative}
    .ls-page:before{content:"";position:absolute;inset:0;pointer-events:none;opacity:.24;background:radial-gradient(ellipse at 7% 100%,#536044 0 3%,transparent 12%),radial-gradient(ellipse at 100% 18%,#3c5140 0 2%,transparent 11%),repeating-linear-gradient(0deg,transparent 0 38px,#29312a22 39px 40px)}
    .ls-page:after{content:"";position:absolute;right:0;top:0;width:70px;height:55px;pointer-events:none;background:linear-gradient(140deg,transparent 48%,#4a4a38 49% 52%,transparent 54%),linear-gradient(145deg,transparent 61%,#455345 62% 78%,transparent 80%)}
    .ls-masthead,.ls-storyline,.ls-controls,.ls-ledger{position:relative}
    .ls-masthead{display:flex;justify-content:space-between;align-items:center;border-bottom:1px solid #52594b;padding-bottom:9px;font:700 11px/1.2 Consolas,monospace;letter-spacing:.13em;text-transform:uppercase}
    .ls-mark{color:#b37b59}.ls-count{color:#899182}
    .ls-storyline{display:grid;grid-template-columns:1fr minmax(220px,36%);gap:20px;align-items:center;margin:18px 0 14px}
    .ls-story-copy h1{font:900 clamp(22px,3.5vw,34px)/1.16 Consolas,"Malgun Gothic",monospace;letter-spacing:-.04em;margin:0 0 13px}
    .ls-story-copy p{font-size:clamp(15px,1.7vw,17px);font-weight:600;line-height:1.65;margin:0;max-width:570px}
    .ls-caption{color:#9a9c8d;font:12px/1.6 Consolas,"Malgun Gothic",monospace;margin-top:12px;white-space:pre-line}
    .ls-comic{width:100%;aspect-ratio:2.05;border:1px solid #56604f;background:#0d1210;display:block;box-shadow:4px 4px 0 #080a08;filter:saturate(.72)}
    .ls-rule{height:4px;background:linear-gradient(90deg,#657051 0 8%,#30372f 8% 96%,#70513d 96%);border:0;margin:13px 0}
    .ls-controls{display:grid;grid-template-columns:repeat(5,minmax(0,1fr));gap:7px}
    .ls-control{min-height:56px;border-top:1px solid #596052;border-bottom:1px solid #252b25;padding:7px 8px;font-size:12px;background:linear-gradient(90deg,#20272088,transparent)}
    .ls-control kbd{display:block;color:#c29668;font:700 12px Consolas,monospace;margin-bottom:3px}
    .ls-actions{position:relative;display:flex;flex-wrap:wrap;gap:8px;margin-top:15px}
    .ls-actions button{border:1px solid #626454;background:#293129;color:#d5d3c2;padding:10px 16px;cursor:pointer;font:700 13px Consolas,"Malgun Gothic",monospace;box-shadow:2px 2px 0 #080a08}
    .ls-actions button:hover{background:#42513d}.ls-actions .ls-quiet{background:#161c17;color:#aaa997}
    .ls-record-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));border-top:1px solid #515747;margin-top:20px}
    .ls-record{padding:13px 10px;border-bottom:1px solid #3b4238}.ls-record b{display:block;font:700 25px Consolas,monospace}.ls-record span{font-size:12px;color:#96998a}
    .ls-note{position:relative;color:#96998a;font:11px/1.5 Consolas,"Malgun Gothic",monospace;margin:12px 0 0}
    @media(max-width:650px){.ls-storyline{grid-template-columns:1fr}.ls-comic{aspect-ratio:1.9}.ls-controls{grid-template-columns:repeat(2,minmax(0,1fr))}.ls-story-copy p{font-size:16px}}
  `;
  document.head.append(style);

  const story = [
    { kicker: '[ 시스템 로그 : 04월 17일 ]', title: "전투 슈트에 생존 보조 장치 'NEXUS'를 이식했다.", caption: '첫 실전 평가 결과, 부상자 0명.\n그러나… 단 한 건의 구조 요청은 끝내 미접수 처리되었다.', scene: 0 },
    { kicker: '[ 최후의 훈령 ]', title: '“어떤 희생이 있더라도 인원 손실을 막아라.”', caption: '본부와의 통신이 완전히 끊긴 뒤에도, 그 명령만큼은 시스템의 코어에 지워지지 않았다.', scene: 1 },
    { kicker: '[ NEXUS의 독단적 판단 ]', title: '지상의 탈출로는 더 이상 생존을 보장하지 않는다.', caption: "시스템은 대피 인원을 '완전한 보호 대상'으로 재정의하고,\n모든 출구와 구역을 영구 격리된 '위험 지역'으로 강제 전환했다.\n이제 연구소의 모든 문, 승강기, 그리고 경비 프레임의 권한은 오직 하나의 회로로 모인다.", scene: 2 },
    { kicker: '[ 격리 경보 / 06:12 ]', title: '빛이 완전히 차단된 정비 통로.', caption: "이 지옥의 수동 차단기를 유일하게 알고 있는 기술자 한 명이, 비로소 눈을 떴다.\n\n차가운 어둠 속에서 NEXUS의 경고가 울린다.\n…'E-04' 구역의 발전기 세 개를 모두 가동하면, 봉쇄문이 열릴 것이다.\n물론, 그 시도가 어떤 대가를 치르게 될지 알고 있으면서도.", scene: 3 }
  ];
  const intro = document.createElement('section');
  intro.id = 'ls-intro';
  intro.setAttribute('role', 'dialog');
  intro.setAttribute('aria-modal', 'true');
  intro.setAttribute('aria-label', 'LAST SPARK 이야기와 조작법');
  document.body.append(intro);

  const records = document.createElement('section');
  records.id = 'ls-records';
  records.hidden = true;
  records.setAttribute('role', 'dialog');
  records.setAttribute('aria-modal', 'true');
  document.body.append(records);
  for (const panel of [intro, records]) panel.addEventListener('click', event => {
    if (event.target.closest('button')) window.playLastSparkUiClick?.();
  });

  let page = 0;
  const fmt = value => `${Math.floor(Number(value || 0) / 60)}분 ${Math.floor(Number(value || 0) % 60)}초`;
  const save = () => {
    try { localStorage.setItem(SAVE_KEY, JSON.stringify(stats)); } catch { /* Keep this session usable without storage. */ }
  };
  const drawComic = scene => {
    const canvas = intro.querySelector('.ls-comic');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const w = canvas.width, h = canvas.height;
    ctx.fillStyle = scene === 3 ? '#11191b' : '#192123'; ctx.fillRect(0, 0, w, h);
    ctx.fillStyle = '#202b2c'; ctx.fillRect(0, h * .62, w, h * .38);
    ctx.strokeStyle = '#46514d'; ctx.lineWidth = 5;
    for (let x = 20; x < w; x += 78) { ctx.strokeRect(x, 40, 58, 105); ctx.beginPath(); ctx.moveTo(x + 8, 52); ctx.lineTo(x + 49, 52); ctx.moveTo(x + 8, 134); ctx.lineTo(x + 49, 134); ctx.stroke(); }
    ctx.strokeStyle = '#6d493b'; ctx.lineWidth = 4; ctx.beginPath(); ctx.moveTo(0, h * .68); ctx.bezierCurveTo(w * .25, h * .85, w * .55, h * .7, w * .82, h * .9); ctx.stroke();
    ctx.fillStyle = scene === 2 ? '#7d332e' : '#9b5540'; ctx.fillRect(w * .08, h * .16, w * .23, 5);
    ctx.fillStyle = '#d7caa8'; ctx.font = 'bold 12px Consolas'; ctx.fillText(['DEFENSE SYSTEMS / NEXUS','DIRECTIVE 01','ACCESS DENIED','E-04   /   RESEARCH WING'][scene], 18, h - 19);
    if (scene === 0 || scene === 1 || scene === 3) {
      const drawn = window.LastSparkAssets && window.LastSparkAssets.draw(ctx, 'Player_P1_Idle', .15, w * .68, h * .88, { scale: 1.35, baseScale: .4, fallback: 'Player_P1_Run' });
      if (!drawn) { ctx.fillStyle = '#647c7c'; ctx.fillRect(w * .64, h * .34, 55, 120); ctx.fillStyle = '#d5c8aa'; ctx.fillRect(w * .68, h * .25, 35, 35); }
      ctx.strokeStyle = '#90ded0'; ctx.lineWidth = 3; ctx.beginPath(); ctx.arc(w * .70, h * .56, 18, 0, Math.PI * 2); ctx.stroke();
    }
    if (scene === 2) {
      ctx.fillStyle = '#311f20'; ctx.fillRect(w * .68, h * .18, 88, 177); ctx.strokeStyle = '#9e5244'; ctx.lineWidth = 4; ctx.strokeRect(w * .68, h * .18, 88, 177);
      if (window.LastSparkAssets && !window.LastSparkAssets.draw(ctx, 'Enemy2_Idle', .1, w * .74, h * .88, { scale: .85, baseScale: .4, fallback: 'Enemy2_Run' })) { ctx.fillStyle = '#707676'; ctx.fillRect(w * .71, h * .4, 54, 99); }
      ctx.fillStyle = '#d75648'; ctx.fillRect(w * .70, h * .45, 52, 4); ctx.fillRect(w * .70, h * .51, 38, 4);
    }
    if (scene === 3 && window.LastSparkAssets && !window.LastSparkAssets.draw(ctx, 'BossFinal_Idle', .2, w * .9, h * .88, { scale: .78, baseScale: .4, alpha: .76, fallback: 'BossFinal_Awakening' })) { ctx.fillStyle = '#5d3732'; ctx.fillRect(w * .87, h * .28, 72, 145); }
    ctx.fillStyle = '#050708'; ctx.globalAlpha = .62; ctx.fillRect(0, 0, w, h); ctx.globalAlpha = 1;
    if (scene === 0 || scene === 1 || scene === 3) {
      if (window.LastSparkAssets) window.LastSparkAssets.draw(ctx, 'Player_P1_Idle', .15, w * .68, h * .88, { scale: 1.35, baseScale: .4, fallback: 'Player_P1_Run' });
    }
  };
  const renderIntro = () => {
    const isStory = page < story.length;
    if (isStory) {
      const panel = story[page];
      intro.innerHTML = `<article class="ls-page"><header class="ls-masthead"><span class="ls-mark">LAST SPARK / INCIDENT FILE</span><span class="ls-count">${String(page + 1).padStart(2, '0')} — 04</span></header><div class="ls-storyline"><div class="ls-story-copy"><div class="ls-mark">${panel.kicker}</div><h1>${panel.title}</h1><p class="ls-caption">${panel.caption}</p></div><canvas class="ls-comic" width="660" height="320" aria-label="이야기 장면 ${page + 1}"></canvas></div><div class="ls-rule"></div><div class="ls-actions"><button id="ls-next">${page === story.length - 1 ? '조작 확인' : '다음 기록  →'}</button><button class="ls-quiet" id="ls-skip">이야기 건너뛰기</button></div><p class="ls-note">기록 번호 ${String(page + 1).padStart(2, '0')} · 계속은 Enter, 건너뛰기는 Esc</p></article>`;
      drawComic(panel.scene);
      intro.querySelector('#ls-next').onclick = () => { page += 1; renderIntro(); };
      intro.querySelector('#ls-skip').onclick = () => { page = story.length; renderIntro(); };
      return;
    }
    intro.innerHTML = `<article class="ls-page"><header class="ls-masthead"><span class="ls-mark">LAST SPARK / FIELD NOTES</span><span class="ls-count">조작 지침</span></header><h1 style="font:900 clamp(27px,5vw,42px) Consolas,monospace;margin:22px 0 8px">출동 전 확인</h1><p class="ls-caption">순찰 경비는 조사 동작과 무관하게 움직여. 보관함 근처에서 E를 눌러 숨고, 다시 눌러 나와.</p><div class="ls-rule"></div><div class="ls-controls"><div class="ls-control"><kbd>A / D · ← →</kbd>이동</div><div class="ls-control"><kbd>SPACE</kbd>점프</div><div class="ls-control"><kbd>SHIFT</kbd>대시</div><div class="ls-control"><kbd>J / 클릭</kbd>근접 공격</div><div class="ls-control"><kbd>E</kbd>조사 · 숨기 · 승강기</div><div class="ls-control"><kbd>ENTER</kbd>대화 진행</div><div class="ls-control"><kbd>ESC</kbd>일시정지</div><div class="ls-control"><kbd>Q</kbd>타이틀로 나가기</div><div class="ls-control"><kbd>M</kbd>환경음</div></div><div class="ls-actions"><button id="ls-open-records">기록 확인</button><button class="ls-quiet" id="ls-reset">새 프로필</button><button id="ls-deploy">타이틀 화면으로</button></div><p class="ls-note">기록 동기화: <span id="ls-sync">${syncLabel}</span> · 서버에 연결할 수 없어도 이 브라우저에 계속 저장돼.</p></article>`;
    intro.querySelector('#ls-open-records').onclick = () => { renderRecords(); intro.hidden = true; records.hidden = false; };
    intro.querySelector('#ls-deploy').onclick = () => { intro.hidden = true; };
    intro.querySelector('#ls-reset').onclick = () => {
      if (!window.confirm('로컬 기록을 비우고 새 프로필로 전환할까? 기존 서버 기록은 삭제되지 않아.')) return;
      stats = { ...defaults }; profileId = makeId();
      try { localStorage.setItem(ID_KEY, profileId); } catch { /* Keep the new identity for this session. */ }
      save(); renderIntro();
    };
  };

  const renderRecords = () => {
    records.innerHTML = `<article class="ls-page"><header class="ls-masthead"><span class="ls-mark">LAST SPARK / PILOT LOG</span><span class="ls-count">${syncLabel}</span></header><h1 style="font:900 clamp(27px,5vw,42px) Consolas,monospace;margin:22px 0 8px">플레이 기록</h1><div class="ls-record-grid"><div class="ls-record"><b>${stats.runs}</b><span>출격 횟수</span></div><div class="ls-record"><b>${fmt(stats.playSeconds)}</b><span>누적 플레이 시간</span></div><div class="ls-record"><b>${stats.bestRoom || '—'}구역 · ${stats.bestPhase || '—'}페이즈</b><span>최고 진행 기록</span></div><div class="ls-record"><b>${stats.bestClearSeconds == null ? '—' : fmt(stats.bestClearSeconds)}</b><span>최고 클리어 시간</span></div></div><div class="ls-actions"><button id="ls-back">기록지로</button><button class="ls-quiet" id="ls-close">닫기</button></div></article>`;
    records.querySelector('#ls-back').onclick = () => { records.hidden = true; intro.hidden = false; };
    records.querySelector('#ls-close').onclick = () => { records.hidden = true; intro.hidden = true; };
  };

  async function syncStats(keepalive = false) {
    try {
      const response = await fetch(`${API_BASE}/profiles/sync`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ profileId, profile: { ...stats, playSeconds: Math.floor(stats.playSeconds) } }),
        keepalive
      });
      if (!response.ok) throw new Error('sync failed');
      const remote = await response.json();
      stats = {
        runs: Math.max(stats.runs, Number(remote.runs) || 0),
        playSeconds: Math.max(stats.playSeconds, Number(remote.playSeconds) || 0),
        bestRoom: Math.max(stats.bestRoom, Number(remote.bestRoom) || 0),
        bestPhase: Math.max(stats.bestPhase, Number(remote.bestPhase) || 0),
        bestClearSeconds: stats.bestClearSeconds == null ? remote.bestClearSeconds : remote.bestClearSeconds == null ? stats.bestClearSeconds : Math.min(stats.bestClearSeconds, remote.bestClearSeconds)
      };
      syncLabel = 'MySQL 동기화 완료';
      save();
      const label = document.querySelector('#ls-sync');
      if (label) label.textContent = syncLabel;
    } catch {
      syncLabel = '오프라인 · 브라우저에 저장 중';
      const label = document.querySelector('#ls-sync');
      if (label) label.textContent = syncLabel;
    }
  }

  renderIntro();
  setInterval(() => { if (!intro.hidden && page < story.length) drawComic(story[page].scene); }, 700);
  setTimeout(() => { fetch(`${API_BASE.replace(/\/v1$/, '')}/health`).then(response => response.ok ? response.json() : null).then(health => {
    if (health && health.status === 'ok' && health.database === 'connected') {
      syncLabel = 'MySQL 연결됨';
      const label = document.querySelector('#ls-sync');
      if (label) label.textContent = syncLabel;
    }
    return fetch(`${API_BASE}/profiles/${encodeURIComponent(profileId)}`).then(response => response.ok ? response.json() : null);
  }).then(remote => {
    if (!remote) return;
    stats = { runs: Math.max(stats.runs, remote.runs || 0), playSeconds: Math.max(stats.playSeconds, remote.playSeconds || 0), bestRoom: Math.max(stats.bestRoom, remote.bestRoom || 0), bestPhase: Math.max(stats.bestPhase, remote.bestPhase || 0), bestClearSeconds: stats.bestClearSeconds == null ? remote.bestClearSeconds : remote.bestClearSeconds == null ? stats.bestClearSeconds : Math.min(stats.bestClearSeconds, remote.bestClearSeconds) };
    syncLabel = 'MySQL 동기화 완료'; save();
  }).catch(() => {
    if (location.protocol === 'file:') syncLabel = 'DB 미연결 · http://localhost:8080 사용';
    const label = document.querySelector('#ls-sync');
    if (label) label.textContent = syncLabel;
  }); }, 0);

  let runStartedAt = 0, runPlaySeconds = 0, lastTickAt = 0, lastSaveAt = 0;
  setInterval(() => {
    let state;
    try { state = typeof window.getLastSparkState === 'function' ? window.getLastSparkState() : null; } catch { state = null; }
    if (!state || !intro.hidden || !records.hidden) return;
    const mode = String(state.mode || ''), now = Date.now();
    if (mode === 'play' && !runStartedAt) { runStartedAt = now; runPlaySeconds = 0; lastTickAt = now; stats.runs += 1; save(); }
    if (mode === 'play' && runStartedAt) {
      const delta = Math.max(0, Math.min(2, (now - lastTickAt) / 1000));
      runPlaySeconds += delta; stats.playSeconds += delta; lastTickAt = now;
      const room = Number(state.room), phase = Number(state.maxPhase);
      if (Number.isFinite(room)) stats.bestRoom = Math.max(stats.bestRoom, room + 1);
      if (Number.isFinite(phase)) stats.bestPhase = Math.max(stats.bestPhase, phase);
      if (now - lastSaveAt > 5000) { save(); syncStats(); lastSaveAt = now; }
    }
    if (runStartedAt && mode === 'ending') {
      const clearSeconds = Math.max(1, Math.round(runPlaySeconds));
      stats.bestClearSeconds = stats.bestClearSeconds == null ? clearSeconds : Math.min(stats.bestClearSeconds, clearSeconds);
      runStartedAt = 0; runPlaySeconds = 0; save(); syncStats();
    } else if (runStartedAt && (mode === 'dead' || mode === 'title')) {
      runStartedAt = 0; runPlaySeconds = 0; save(); syncStats();
    }
  }, 1000);

  document.addEventListener('keydown', event => {
    if (!intro.hidden) {
      if (event.key === 'Escape') { window.playLastSparkUiClick?.(); page = story.length; renderIntro(); event.preventDefault(); }
      else if ((event.key === 'Enter' || event.key === ' ') && page < story.length) { window.playLastSparkUiClick?.(); page += 1; renderIntro(); event.preventDefault(); }
      event.stopPropagation();
      return;
    }
    if (!records.hidden) {
      if (event.key === 'Escape' || event.key === 'F2') { records.hidden = true; event.preventDefault(); }
      event.stopPropagation();
      return;
    }
    if (event.key === 'F2') { renderRecords(); records.hidden = !records.hidden; }
  }, true);
  document.addEventListener('visibilitychange', () => { save(); if (document.hidden) syncStats(true); });
  window.addEventListener('pagehide', () => { save(); syncStats(true); });
})();
