import test from 'node:test';
import assert from 'node:assert/strict';
import {applyPoison,tickPoison,tickMelee} from './combat.js';

test('poison deals twelve damage over three seconds then expires',()=>{
 const p={hp:100};applyPoison(p);let total=0;
 for(let i=0;i<90;i++)total+=tickPoison(p,1/30);
 assert.equal(total,12);assert.equal(p.hp,88);
 assert.equal(tickPoison(p,2),0);
});
test('poison refreshes duration without stacking or an instant tick',()=>{
 const p={hp:100};applyPoison(p);tickPoison(p,1);
 applyPoison(p);assert.equal(p.poison,3);
 assert.equal(tickPoison(p,.1),0);
 assert.equal(tickPoison(p,2.9),12);
 assert.equal(p.hp,84);
});
test('skeletons telegraph, strike only in melee reach, and respect cooldown',()=>{
 const e={};assert.equal(tickMelee(e,40,.01),false);assert.equal(e.windup,.4);
 assert.equal(tickMelee(e,40,.2),false);
 assert.equal(tickMelee(e,40,.21),true);
 assert.equal(tickMelee(e,40,.1),false);assert.equal(e.windup,0);
 const escaped={};tickMelee(escaped,40,.01);
 assert.equal(tickMelee(escaped,90,.5),false);
 const distant={};assert.equal(tickMelee(distant,200,1),false);assert.equal(distant.windup,undefined);
});
