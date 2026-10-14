export function applyPoison(player){player.poison=3;player.poisonTick=0;}
export function tickPoison(player,dt){
 const elapsed=Math.min(dt,player.poison||0);
 player.poison=Math.max(0,(player.poison||0)-dt);
 player.poisonTick=(player.poisonTick||0)+elapsed;
 const ticks=Math.floor((player.poisonTick+1e-9)/.5);
 player.poisonTick-=ticks*.5;
 const damage=ticks*2;player.hp-=damage;return damage;
}
// Skeletons telegraph a sword swing; only a completed swing in reach can hit.
export function tickMelee(enemy,distance,dt){
 enemy.meleeCooldown=Math.max(0,(enemy.meleeCooldown||0)-dt);
 enemy.swing=Math.max(0,(enemy.swing||0)-dt);
 if(enemy.windup>0){enemy.windup=Math.max(0,enemy.windup-dt);if(enemy.windup===0){enemy.meleeCooldown=.9;enemy.swing=.18;return distance<58;}return false;}
 if(distance<52&&enemy.meleeCooldown===0)enemy.windup=.4;
 return false;
}
