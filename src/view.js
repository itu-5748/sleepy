// Fixed angled camera. Simulation remains in world space; sprites stay upright.
export const VIEW={width:1000,height:640,zoom:1.15,anchorX:500,anchorY:350};
export function project(point,camera,height=0){const dx=point.x-camera.x,dy=point.y-camera.y;return {x:VIEW.anchorX+(dx+dy)*.866*VIEW.zoom,y:VIEW.anchorY+(dy-dx)*.5*VIEW.zoom-height};}
export function screenVector(x,y){return {x:(x/.866-y/.5)/2,y:(x/.866+y/.5)/2};}
export function unproject(point,camera,height=0){const v=screenVector((point.x-VIEW.anchorX)/VIEW.zoom,(point.y+height-VIEW.anchorY)/VIEW.zoom);return {x:camera.x+v.x,y:camera.y+v.y};}
export const viewCamera=player=>({x:player.x,y:player.y});
