// Original hand-authored pixel sprites. Each cell is one pixel at the chosen scale.
const palette={o:'#18202a',s:'#657787',h:'#b9ccd5',w:'#e8edf0',b:'#354758',r:'#9d3845',g:'#d9b36a',k:'#161920',v:'#536338',p:'#9ae35e',d:'#baa988',c:'#eee1be'};
const knight=[
'.......rr.......','......rrr.......','.....oooooo.....','....ohhhhsho....','....ohhhhsso....','....oskkksso....','....osgsksso....','.....osssso.....','...ooohhhooo....','..oshhhhhhsso...','..oshhghhhsso...','..osshhhhssoo...','...obsssshbo....','...obbggbbbo....','....obbbbo......','....ossso.......','....ososo.......','...ohsoshso.....','...ooooooo......'
];
const skeleton=[
'.....oooooo.....','....occccddo....','....ockkckdo....','....ockkckdo....','.....occcdo.....','.....ockkdo.....','......oddo......','...ooocddoooo...','..ococccddoddo..','..odoookkoddo...','..odocccddodo...','...oookkoooo....','....occcdo......','.....oddo.......','....ododdo......','....odoodo......','....odo.odo.....','...oddo.oddo....','...ooo..ooo.....'
];
const spider=[
'..oo............oo..','...oo..........oo...','.oo.oo..oooo..oo.oo.','..oo.ooovvvvooo.oo..','...oooovvvvvvoooo...','oooooovvppvvvvoooooo','...oovvppppvvvvoo...','ooooovvvvvvvvvvooooo','...ooovvvvvvvvooo...','..oo.ooovvvvooo.oo..','.oo.oo..oooo..oo.oo.','...oo....pp....oo...','..oo.....pp.....oo..'
];
function pixels(ctx,rows,x,y,scale,flash=false){
 const width=Math.max(...rows.map(r=>r.length));
 for(let j=0;j<rows.length;j++)for(let i=0;i<rows[j].length;i++)if(palette[rows[j][i]]){ctx.fillStyle=flash?'#fff4d0':palette[rows[j][i]];ctx.fillRect(Math.round(x-width*scale/2+i*scale),Math.round(y-rows.length*scale/2+j*scale),scale,scale);}
}
function shadow(ctx,x,y,width){ctx.fillStyle='#0005';ctx.fillRect(Math.round(x-width/2),Math.round(y+14),width,7);}
function sword(ctx,x,y,flip,swing=false){ctx.save();ctx.translate(Math.round(x),Math.round(y));ctx.scale(flip,1);ctx.fillStyle='#d9b36a';ctx.fillRect(13,3,12,3);ctx.fillStyle='#b9ccd5';ctx.fillRect(swing?24:18,swing?-2:-23,4,swing?13:26);ctx.fillStyle='#e8edf0';ctx.fillRect(swing?28:22,swing?-2:-23,2,swing?13:26);ctx.fillStyle='#493a2c';ctx.fillRect(18,6,4,8);ctx.restore();}
export function drawKnight(ctx,p,{time=0,moving=false,angle=0,weapon='blade',attacking=false,poisoned=false}={}){
 const bob=moving?Math.floor(Math.sin(time*18)*2):0,flip=Math.cos(angle)<0?-1:1;
 shadow(ctx,p.x,p.y,34);ctx.save();ctx.translate(Math.round(p.x),Math.round(p.y+bob));ctx.scale(flip,1);pixels(ctx,knight,0,-2,2);
 // Heraldic shield and visible equipped weapon.
 ctx.fillStyle='#d9b36a';ctx.fillRect(-24,-3,12,16);ctx.fillRect(-22,13,8,4);ctx.fillStyle='#325c78';ctx.fillRect(-22,-1,8,13);ctx.fillStyle='#e8edf0';ctx.fillRect(-19,1,2,9);ctx.fillRect(-22,4,8,2);
 if(weapon==='spear'){ctx.fillStyle='#84664d';ctx.fillRect(18,-29,3,48);ctx.fillStyle='#e8edf0';ctx.fillRect(16,-34,7,8);}else if(weapon==='orb'){ctx.fillStyle='#d9b36a';ctx.fillRect(18,-15,3,32);ctx.fillStyle='#edc799';ctx.fillRect(14,-22,11,9);}else sword(ctx,0,0,1,attacking);
 if(poisoned){ctx.fillStyle='#9ae35e';ctx.fillRect(-6,-27,3,3);ctx.fillRect(6,-31,3,3);}ctx.restore();
}
export function drawEnemy(ctx,e,time=0){
 shadow(ctx,e.x,e.y,e.boss?60:e.shooter?46:30);
 const bob=Math.floor(Math.sin(time*(e.shooter?20:12)+e.x)*2);
 if(e.shooter){pixels(ctx,spider,e.x,e.y+bob,2,e.flash>0);if(e.fire<.35){ctx.fillStyle='#b9ff7b';ctx.fillRect(Math.round(e.x-4),Math.round(e.y+9),8,5);}}
 else {const scale=e.boss?3:2;ctx.save();ctx.translate(Math.round(e.x),Math.round(e.y+bob));ctx.scale(e.facing<0?-1:1,1);pixels(ctx,skeleton,0,-2,scale,e.flash>0);sword(ctx,0,0,1,e.windup>0||e.swing>0);if(e.boss){ctx.fillStyle='#d9b36a';ctx.fillRect(-10,-38,20,5);for(let i=-10;i<=6;i+=8)ctx.fillRect(i,-44,4,6);}ctx.restore();}
 if(e.windup>0){ctx.strokeStyle='#e9ae71';ctx.lineWidth=2;ctx.beginPath();ctx.arc(e.x,e.y,52,0,Math.PI*2);ctx.stroke();}
 ctx.fillStyle='#253128';ctx.fillRect(Math.round(e.x-e.r),Math.round(e.y-(e.boss?46:30)),e.r*2,3);ctx.fillStyle=e.shooter?'#9ae35e':'#c9ad81';ctx.fillRect(Math.round(e.x-e.r),Math.round(e.y-(e.boss?46:30)),e.r*2*e.hp/e.max,3);
}
