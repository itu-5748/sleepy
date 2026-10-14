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
export function createMap(random=Math.random){
 const camps=[];
 for(let region=0;region<5;region++)for(let i=0;i<3;i++){
  const x=regions[region].start+420+i*240;
  const y=550+(i===1?-220:120)*(region%2?-1:1);
  camps.push({x,y,region,active:false,enemies:Array.from({length:region===4?3:3+region},(_,j)=>({x:x+(random()-.5)*160,y:y+(random()-.5)*160,shooter:j%3===2}))});
 }
 return {camps,shrines:[900,1950,3000,4100].map((x,i)=>({x,y:i%2?760:330,used:false})),boss:{x:5050,y:550,active:false}};
}
