import test from 'node:test';
import assert from 'node:assert/strict';
import {project,unproject,screenVector,viewCamera,VIEW} from './view.js';

test('angled projection and inverse preserve aim across the map',()=>{
 for(const camera of [{x:150,y:550},{x:3000,y:760},{x:5050,y:550}])for(const target of [{x:900,y:330},{x:2300,y:590},{x:5050,y:550}]){
  const screen=project(target,camera,16),world=unproject(screen,camera,16);
  assert.ok(Math.abs(world.x-target.x)<1e-8);
  assert.ok(Math.abs(world.y-target.y)<1e-8);
 }
});
test('camera follows the knight and keeps its feet at the screen anchor',()=>{
 const p={x:2200,y:600};assert.deepEqual(project(p,viewCamera(p)),{x:VIEW.anchorX,y:VIEW.anchorY});
});
test('keyboard direction projects to the requested screen direction',()=>{
 for(const [x,y] of [[1,0],[0,-1],[1,1],[-1,0]]){
  const vector=screenVector(x,y),p=project(vector,{x:0,y:0});
  assert.ok(Math.abs(p.x-VIEW.anchorX-x*VIEW.zoom)<1e-8);
  assert.ok(Math.abs(p.y-VIEW.anchorY-y*VIEW.zoom)<1e-8);
 }
});
