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

test('terrain blocks walking and high-speed dashes while allowing wall sliding',async()=>{
 const {moveOnPath,isWalkable}=await import('./world.js');
 const body={x:150,y:550};
 moveOnPath(body,0,-1000,14);
 assert.ok(body.y>400);
 assert.ok(isWalkable(body.x,body.y,14));
 const slider={x:940,y:430};
 moveOnPath(slider,100,40,14);
 assert.ok(slider.y>430);
 assert.ok(isWalkable(slider.x,slider.y,14));
 assert.equal(isWalkable(150,50,14),false);
});

test('all shrines, enemy spawns, and boss connect to the starting path',async()=>{
 const {isWalkable}=await import('./world.js');
 const step=20,start=[8,28],seen=new Set([start.join(',')]),queue=[start];
 for(let head=0;head<queue.length;head++){
  const [x,y]=queue[head];
  for(const [dx,dy] of [[1,0],[-1,0],[0,1],[0,-1]]){
   const nx=x+dx,ny=y+dy,key=nx+','+ny;
   if(!seen.has(key)&&isWalkable(nx*step,ny*step,14)){seen.add(key);queue.push([nx,ny]);}
  }
 }
 const map=createMap(()=>.95);
 for(const point of [...map.shrines,map.boss,...map.camps.flatMap(c=>c.enemies)]){
  assert.ok(isWalkable(point.x,point.y,20),'spawn must fit within path');
  assert.ok(seen.has(Math.round(point.x/step)+','+Math.round(point.y/step)),'destination must be reachable');
 }
});

test('attacks cannot cross impassable terrain',async()=>{
 const {lineOnPath}=await import('./world.js');
 assert.equal(lineOnPath({x:150,y:550},{x:250,y:550}),true);
 assert.equal(lineOnPath({x:150,y:550},{x:150,y:50}),false);
});
