export const WORLD = {width:5400,height:1100};
export const regions = [
  {name:'ASHEN APPROACH',start:0,end:1000},
  {name:'WHISPERING GROVE',start:1000,end:2100},
  {name:'BROKEN CAUSEWAY',start:2100,end:3200},
  {name:'CINDER WILDS',start:3200,end:4300},
  {name:'WARDEN’S SANCTUM',start:4300,end:5400}
];
export function regionAt(x) {
 const index=regions.findIndex(r=>x<r.end);
 return index===-1 ? regions.length-1 : index;
}
export function cameraAt(player){return {x:Math.max(0,Math.min(WORLD.width-1000,player.x-500)),y:Math.max(0,Math.min(WORLD.height-640,player.y-320))};}
export const roadY = x => 550+Math.sin(x/500)*90;
const shrineSites=[900,1950,3000,4100].map((x,i)=>({x,y:i%2?760:330}));
export const paths=[];
for(let x=60;x<5340;x+=60) paths.push({a:{x,y:roadY(x)},b:{x:Math.min(x+60,5340),y:roadY(Math.min(x+60,5340))},width:160});
for(const s of shrineSites) paths.push({a:{x:s.x,y:roadY(s.x)},b:s,width:110});
export const clearings=[{x:150,y:550,r:130},{x:5050,y:550,r:210},...shrineSites.map(s=>({...s,r:90}))];
for(const region of regions) for(let i=0;i<3;i++){const x=region.start+420+i*240;clearings.push({x,y:roadY(x),r:140});}
function segmentDistance(x,y,a,b){const dx=b.x-a.x,dy=b.y-a.y,t=Math.max(0,Math.min(1,((x-a.x)*dx+(y-a.y)*dy)/(dx*dx+dy*dy||1)));return Math.hypot(x-a.x-t*dx,y-a.y-t*dy);}
function onGround(x,y){return x>=24&&x<=WORLD.width-24&&y>=24&&y<=WORLD.height-24&&(clearings.some(c=>Math.hypot(x-c.x,y-c.y)<=c.r)||paths.some(p=>segmentDistance(x,y,p.a,p.b)<=p.width/2));}
export function isWalkable(x,y,radius=0){
 if(!onGround(x,y))return false;
 for(let i=0;i<12&&radius>0;i++){const a=i*Math.PI/6;if(!onGround(x+Math.cos(a)*radius,y+Math.sin(a)*radius))return false;}
 return true;
}
export function moveOnPath(body,dx,dy,radius=14){
 const steps=Math.max(1,Math.ceil(Math.hypot(dx,dy)/4));
 for(let i=0;i<steps;i++){
  const x=dx/steps,y=dy/steps;
  if(isWalkable(body.x+x,body.y+y,radius)){body.x+=x;body.y+=y;}
  else {if(isWalkable(body.x+x,body.y,radius))body.x+=x;if(isWalkable(body.x,body.y+y,radius))body.y+=y;}
 }
}
export function lineOnPath(a,b){const n=Math.max(1,Math.ceil(Math.hypot(b.x-a.x,b.y-a.y)/8));for(let i=0;i<=n;i++)if(!isWalkable(a.x+(b.x-a.x)*i/n,a.y+(b.y-a.y)*i/n))return false;return true;}
export function createMap(random=Math.random){
 const camps=[];
 for(let region=0;region<5;region++)for(let i=0;i<3;i++){
  const x=regions[region].start+420+i*240;
  const y=roadY(x);
  camps.push({x,y,region,active:false,enemies:Array.from({length:region===4?3:3+region},(_,j)=>({x:x+(random()-.5)*120,y:y+(random()-.5)*120,shooter:j%3===2}))});
 }
 return {camps,shrines:shrineSites.map(s=>({...s,used:false})),boss:{x:5050,y:550,active:false}};
}
