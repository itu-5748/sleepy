export const freshSave = () => ({version:1,runs:0,embers:0,stats:{health:0,power:0,haste:0},weapon:'blade'});
export function readSave(storage) {
  try {
    const s=JSON.parse(storage.getItem('emberbound'));
    if(s?.version!==1) return freshSave();
    const clean=freshSave();
    for(const key of ['runs','embers']) clean[key]=Math.max(0,Math.floor(Number(s[key])||0));
    for(const key of Object.keys(clean.stats)) clean.stats[key]=Math.max(0,Math.min(200,Math.floor(Number(s.stats?.[key])||0)));
    clean.weapon=['blade','spear','orb'].includes(s.weapon)?s.weapon:'blade';
    return clean;
  } catch {return freshSave();}
}
export function gainsForRun(run) {
  return {health:Math.min(3,Math.floor(run.damageTaken/80)),power:Math.min(3,Math.floor(run.kills/8)),haste:Math.min(3,Math.floor(run.dodges/6)),embers:Math.max(1,Math.floor(run.kills/5)+run.rooms*2)};
}
export function settleRun(save,run) {
  const gains=gainsForRun(run);
  const next={...save,runs:save.runs+1,embers:save.embers+gains.embers,stats:{...save.stats}};
  for(const key of Object.keys(next.stats)) next.stats[key]=Math.min(200,next.stats[key]+gains[key]);
  return {save:next,gains};
}
export function buyStat(save,key) {
  if(!Object.hasOwn(save.stats,key)||save.embers<3||save.stats[key]>=200) return save;
  return {...save,embers:save.embers-3,stats:{...save.stats,[key]:save.stats[key]+1}};
}
