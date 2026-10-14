import {WORLD,paths,clearings,isWalkable} from './world.js';
import {project,unproject,VIEW} from './view.js';
import {drawKnight,drawEnemy} from './sprites.js';
let ground,cliffs;
const hash=(x,y)=>{const n=Math.sin(x*127.1+y*311.7)*43758.5453;return n-Math.floor(n);};
function land(ctx){ctx.fillStyle='#fff';ctx.strokeStyle='#fff';ctx.lineCap='round';for(const p of paths){ctx.lineWidth=p.width;ctx.beginPath();ctx.moveTo(p.a.x,p.a.y);ctx.lineTo(p.b.x,p.b.y);ctx.stroke();}for(const c of clearings){ctx.beginPath();ctx.arc(c.x,c.y,c.r,0,Math.PI*2);ctx.fill();}}
function bake(){
 ground=document.createElement('canvas');ground.width=WORLD.width;ground.height=WORLD.height;const g=ground.getContext('2d');land(g);g.globalCompositeOperation='source-atop';g.fillStyle='#504f43';g.fillRect(0,0,WORLD.width,WORLD.height);
 for(let y=0;y<WORLD.height;y+=26)for(let x=-42;x<WORLD.width;x+=42){const px=x+(y/26%2)*21,n=hash(x,y);g.fillStyle=['#717061','#636657','#7c7865','#595e51','#8b826c'][Math.floor(n*5)];g.fillRect(px+2,y+2,38,22);g.fillStyle='#96947b66';g.fillRect(px+3,y+2,36,2);g.fillStyle='#282f2b77';g.fillRect(px+39,y+5,2,18);if(n<.18){g.fillStyle='#526345';g.fillRect(px+2,y+16,10,7);g.fillStyle='#6d7c4b';g.fillRect(px+5,y+18,4,3);}if(n>.88){g.fillStyle='#333b32';g.fillRect(px+18,y+8,2,8);g.fillRect(px+20,y+14,7,2);}}
 cliffs=document.createElement('canvas');cliffs.width=WORLD.width;cliffs.height=WORLD.height;const c=cliffs.getContext('2d');land(c);c.globalCompositeOperation='source-atop';c.fillStyle='#252f2e';c.fillRect(0,0,WORLD.width,WORLD.height);for(let y=0;y<WORLD.height;y+=32)for(let x=0;x<WORLD.width;x+=40){c.fillStyle=hash(x,y)>.5?'#37403a':'#41483e';c.fillRect(x+2,y+2,36,27);c.fillStyle='#151f22';c.fillRect(x+2,y+29,38,3);}
}
function plane(ctx,camera,drop=0){ctx.setTransform(.866*VIEW.zoom,-.5*VIEW.zoom,.866*VIEW.zoom,.5*VIEW.zoom,VIEW.anchorX-(camera.x+camera.y)*.866*VIEW.zoom,VIEW.anchorY+(camera.x-camera.y)*.5*VIEW.zoom+drop);}
function diamond(ctx,x,y,w,h,color){ctx.fillStyle=color;ctx.beginPath();ctx.moveTo(x,y-h);ctx.lineTo(x+w,y);ctx.lineTo(x,y+h);ctx.lineTo(x-w,y);ctx.closePath();ctx.fill();}
function shadow(ctx,p,width,height=12){ctx.fillStyle='#0005';ctx.beginPath();ctx.ellipse(p.x+18,p.y+8,width,height,.4,0,Math.PI*2);ctx.fill();}
function tree(ctx,p,seed){
 shadow(ctx,p,38,14);ctx.fillStyle='#302d25';ctx.fillRect(p.x-6,p.y-44,12,45);ctx.fillStyle='#55483a';ctx.fillRect(p.x-4,p.y-40,3,39);
 for(let tier=0;tier<4;tier++){const top=p.y-115+tier*19,w=18+tier*8;for(let row=0;row<8;row++){const rw=4+row*w/8;ctx.fillStyle=tier%2?'#1b332a':'#234033';ctx.fillRect(Math.round(p.x-rw),top+row*5,Math.round(rw*2),5);ctx.fillStyle='#304b36';ctx.fillRect(Math.round(p.x-rw),top+row*5,Math.round(rw*.75),3);if(row%3===seed%3){ctx.fillStyle='#466044';ctx.fillRect(Math.round(p.x-rw+4),top+row*5,6,3);}}}
}
function pillar(ctx,p){shadow(ctx,p,17,6);diamond(ctx,p.x,p.y,18,9,'#484c40');ctx.fillStyle='#505447';ctx.fillRect(p.x-12,p.y-49,13,48);ctx.fillStyle='#343c34';ctx.fillRect(p.x+1,p.y-49,11,48);for(let y=0;y<3;y++){ctx.fillStyle='#1e2925';ctx.fillRect(p.x-12,p.y-12-y*14,24,2);}diamond(ctx,p.x,p.y-49,16,8,'#8c8b72');ctx.fillStyle='#607245';ctx.fillRect(p.x-13,p.y-10,6,8);}
function fire(ctx,p,time){const flicker=Math.sin(time*9+p.x)*2;const glow=ctx.createRadialGradient(p.x,p.y-42,2,p.x,p.y-42,65);glow.addColorStop(0,'#ffbd5344');glow.addColorStop(1,'#ffbd5300');ctx.fillStyle=glow;ctx.fillRect(p.x-65,p.y-107,130,130);ctx.fillStyle='#352d25';ctx.fillRect(p.x-3,p.y-34,6,34);ctx.fillStyle='#b07837';ctx.fillRect(p.x-8,p.y-36,16,5);ctx.fillStyle='#d8762c';ctx.fillRect(p.x-6,p.y-50+flicker,12,14);ctx.fillStyle='#ffd07b';ctx.fillRect(p.x-3,p.y-57+flicker,6,19);ctx.fillStyle='#fff2bd';ctx.fillRect(p.x-1,p.y-43,3,7);}
function shrine(ctx,p,s,time){
 shadow(ctx,p,48,14);for(let i=0;i<3;i++){diamond(ctx,p.x,p.y-i*7,52-i*9,24-i*4,i%2?'#78745e':'#535c4b');}pillar(ctx,{x:p.x-34,y:p.y-14});pillar(ctx,{x:p.x+34,y:p.y-14});
 ctx.fillStyle=s.used?'#657159':'#dfb970';ctx.fillRect(p.x-5,p.y-80,10,53);ctx.fillRect(p.x-19,p.y-67,38,6);diamond(ctx,p.x,p.y-82,12,15,s.used?'#7a846c':'#fff1b2');if(!s.used){const glow=ctx.createRadialGradient(p.x,p.y-60,5,p.x,p.y-60,100);glow.addColorStop(0,'#ffcd6c55');glow.addColorStop(1,'#ffcd6c00');ctx.fillStyle=glow;ctx.fillRect(p.x-100,p.y-160,200,200);fire(ctx,{x:p.x-48,y:p.y+5},time);fire(ctx,{x:p.x+48,y:p.y+5},time);}
 ctx.fillStyle='#f1d69b';ctx.textAlign='center';ctx.font='11px monospace';ctx.fillText(s.used?'BLESSING CLAIMED':'E · SHRINE',p.x,p.y-105);
}
const trees=[];for(let i=0;i<720;i++){const x=45+hash(i,1)*(WORLD.width-90),y=20+hash(i,2)*(WORLD.height-40);if(!isWalkable(x,y)&&!isWalkable(x-30,y)&&!isWalkable(x+30,y)&&!isWalkable(x,y-30)&&!isWalkable(x,y+30))trees.push({x,y,seed:i});}
export function renderScene(ctx,{camera,player,world,enemies,shots,particles,run,weapon,attacking,moving,invuln,shake,time}){
 if(!ground)bake();ctx.save();ctx.setTransform(1,0,0,1,0,0);ctx.fillStyle='#0e1b20';ctx.fillRect(0,0,1000,640);
 // The world is raised above a shaded, misty ravine.
 const mist=ctx.createLinearGradient(0,0,0,640);mist.addColorStop(0,'#172f31');mist.addColorStop(1,'#0b171c');ctx.fillStyle=mist;ctx.fillRect(0,0,1000,640);
 const quake={x:(Math.random()-.5)*shake,y:(Math.random()-.5)*shake};
 const corners=[{x:-160,y:-160},{x:1160,y:-160},{x:-160,y:800},{x:1160,y:800}].map(p=>unproject(p,camera));
 const sx=Math.max(0,Math.floor(Math.min(...corners.map(p=>p.x)))),sy=Math.max(0,Math.floor(Math.min(...corners.map(p=>p.y))));
 const sw=Math.max(0,Math.min(WORLD.width,Math.ceil(Math.max(...corners.map(p=>p.x))))-sx),sh=Math.max(0,Math.min(WORLD.height,Math.ceil(Math.max(...corners.map(p=>p.y))))-sy);
 const paint=image=>{if(sw&&sh)ctx.drawImage(image,sx,sy,sw,sh,sx,sy,sw,sh);};
 for(let drop=88;drop>=8;drop-=16){plane(ctx,camera,drop+quake.y);ctx.globalAlpha=drop===88?.55:1;paint(cliffs);}ctx.globalAlpha=1;plane(ctx,camera,quake.y);paint(ground);ctx.setTransform(1,0,0,1,quake.x,quake.y);
 const visible=p=>p.x>-160&&p.x<1160&&p.y>-150&&p.y<840;
 const objects=[];
 for(const t of trees){const p=project(t,camera);if(visible(p))objects.push({depth:p.y,draw:()=>{ctx.globalAlpha=player&&Math.abs(p.x-500)<65&&p.y>350&&p.y<460?.35:1;tree(ctx,p,t.seed);ctx.globalAlpha=1;}});}
 for(const c of clearings){for(const side of [-1,1]){const p=project({x:c.x+side*100,y:c.y-45},camera);if(visible(p))objects.push({depth:p.y,draw:()=>{pillar(ctx,p);fire(ctx,{x:p.x,y:p.y-49},time);}});}}
 if(world){for(const s of world.shrines){const p=project(s,camera);if(visible(p))objects.push({depth:p.y,draw:()=>shrine(ctx,p,s,time)});}const b=project(world.boss,camera);if(visible(b))objects.push({depth:b.y-100,draw:()=>{ctx.fillStyle='#c9a275';ctx.font='14px Georgia';ctx.textAlign='center';ctx.fillText('THE ASH WARDEN',b.x,b.y-100);}});}
 for(const e of enemies){const p=project(e,camera);if(visible(p))objects.push({depth:p.y,draw:()=>{shadow(ctx,p,e.boss?32:20,7);if(e.windup>0){ctx.strokeStyle='#e9ae71';ctx.lineWidth=2;ctx.beginPath();ctx.ellipse(p.x,p.y,52*.866*VIEW.zoom*Math.SQRT2,52*.5*VIEW.zoom*Math.SQRT2,0,0,Math.PI*2);ctx.stroke();}drawEnemy(ctx,{...e,windup:0,swing:e.windup>0?.3:e.swing,x:p.x,y:p.y-(e.boss?25:17)},time);}});}
 if(player){const p=project(player,camera);objects.push({depth:p.y,draw:()=>{shadow(ctx,p,23,8);ctx.globalAlpha=invuln>0?.65:1;ctx.save();ctx.translate(p.x,p.y-26);ctx.scale(1.35,1.35);drawKnight(ctx,{...player,x:0,y:0},{time,moving,angle:Math.atan2(attacking.mouse.y-(p.y-20),attacking.mouse.x-p.x),weapon,attacking:attacking.active,poisoned:player.poison>0});ctx.restore();ctx.globalAlpha=1;ctx.fillStyle='#26332c';ctx.fillRect(p.x-27,p.y-65,54,4);ctx.fillStyle=player.poison>0?'#9ae35e':'#d3c698';ctx.fillRect(p.x-27,p.y-65,54*Math.max(0,player.hp/player.max),4);}});}
 for(const s of shots){const p=project(s,camera,16);objects.push({depth:p.y+16,draw:()=>{ctx.fillStyle=s.friendly?'#f4d291':s.poison?'#80cf42':'#ce7559';ctx.fillRect(Math.round(p.x-5),Math.round(p.y-5),10,10);ctx.fillStyle=s.poison?'#d2ff85':'#fff0cc';ctx.fillRect(Math.round(p.x-2),Math.round(p.y-2),4,4);}});}
 objects.sort((a,b)=>a.depth-b.depth);for(const o of objects)o.draw();
 for(const p of particles){ctx.globalAlpha=Math.max(0,p.life/.45);ctx.fillStyle=p.color;ctx.strokeStyle=p.color;if(p.arc){ctx.lineWidth=8;ctx.beginPath();for(let i=0;i<=16;i++){const a=p.ang-.8+i*.1,point=project({x:p.x+Math.cos(a)*p.range*.8,y:p.y+Math.sin(a)*p.range*.8},camera,14);if(i===0)ctx.moveTo(point.x,point.y);else ctx.lineTo(point.x,point.y);}ctx.stroke();}else{const point=project(p,camera,16);ctx.fillRect(Math.round(point.x),Math.round(point.y),3,3);}}
 ctx.globalAlpha=1;const vignette=ctx.createRadialGradient(500,320,190,500,320,620);vignette.addColorStop(0,'#0000');vignette.addColorStop(1,'#030b1288');ctx.fillStyle=vignette;ctx.fillRect(0,0,1000,640);ctx.restore();
}
