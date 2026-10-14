import test from 'node:test';
import assert from 'node:assert/strict';
import {WORLD,regions,regionAt,cameraAt,createMap} from './world.js';

test('regions cover a connected world, including both edges',()=>{
  assert.equal(regions[0].start,0);
  assert.equal(regions.at(-1).end,WORLD.width);
  for(let i=1;i<regions.length;i++) assert.equal(regions[i].start,regions[i-1].end);
  assert.equal(regionAt(999),0);
  assert.equal(regionAt(1000),1);
  assert.equal(regionAt(WORLD.width),4);
});
test('camera follows the player and never reveals beyond world edges',()=>{
  assert.deepEqual(cameraAt({x:150,y:100}),{x:0,y:0});
  assert.deepEqual(cameraAt({x:2000,y:550}),{x:1500,y:230});
  assert.deepEqual(cameraAt({x:WORLD.width,y:WORLD.height}),{x:WORLD.width-1000,y:WORLD.height-640});
});
test('new maps reset encounters and shrines, with reachable enemies',()=>{
  const map=createMap(()=>.5);
  assert.equal(map.camps.length,15);
  assert.equal(map.shrines.length,4);
  for(const camp of map.camps){
    assert.equal(camp.active,false);
    assert.ok(camp.enemies.length>0);
    for(const e of camp.enemies){assert.ok(e.x>0&&e.x<WORLD.width);assert.ok(e.y>0&&e.y<WORLD.height);}
  }
  map.shrines[0].used=true;
  assert.equal(createMap().shrines[0].used,false);
  assert.equal(map.boss.active,false);
});
