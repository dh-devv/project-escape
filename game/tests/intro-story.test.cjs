'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '..', 'enhance.js'), 'utf8');
const match = source.match(/const story = (\[[\s\S]*?\n  \]);/);
assert.ok(match, 'opening comic panels are defined');
const story = vm.runInNewContext(`(${match[1]})`);
assert.equal(story.length, 4, 'opening story remains four panels');
assert.equal(story[0].kicker, '[ 시스템 로그 : 04월 17일 ]');
assert.equal(story[0].title, "전투 슈트에 생존 보조 장치 'NEXUS'를 이식했다.");
assert.equal(story[0].caption, '첫 실전 평가 결과, 부상자 0명.\n그러나… 단 한 건의 구조 요청은 끝내 미접수 처리되었다.');
assert.equal(story[1].kicker, '[ 최후의 훈령 ]');
assert.equal(story[1].title, '“어떤 희생이 있더라도 인원 손실을 막아라.”');
assert.equal(story[1].caption, '본부와의 통신이 완전히 끊긴 뒤에도, 그 명령만큼은 시스템의 코어에 지워지지 않았다.');
assert.equal(story[2].kicker, '[ NEXUS의 독단적 판단 ]');
assert.equal(story[2].title, '지상의 탈출로는 더 이상 생존을 보장하지 않는다.');
assert.equal(story[2].caption, "시스템은 대피 인원을 '완전한 보호 대상'으로 재정의하고,\n모든 출구와 구역을 영구 격리된 '위험 지역'으로 강제 전환했다.\n이제 연구소의 모든 문, 승강기, 그리고 경비 프레임의 권한은 오직 하나의 회로로 모인다.");
assert.equal(story[3].kicker, '[ 격리 경보 / 06:12 ]');
assert.equal(story[3].title, '빛이 완전히 차단된 정비 통로.');
assert.equal(story[3].caption, "이 지옥의 수동 차단기를 유일하게 알고 있는 기술자 한 명이, 비로소 눈을 떴다.\n\n차가운 어둠 속에서 NEXUS의 경고가 울린다.\n…'E-04' 구역의 발전기 세 개를 모두 가동하면, 봉쇄문이 열릴 것이다.\n물론, 그 시도가 어떤 대가를 치르게 될지 알고 있으면서도.");
