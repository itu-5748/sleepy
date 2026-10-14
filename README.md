# Emberbound

## Hosting

The GitHub Actions workflow publishes the static game to GitHub Pages on pushes to `main`. In the repository's **Settings → Pages**, select **GitHub Actions** as the source, then run **Deploy Emberbound to GitHub Pages** from the Actions tab if needed. The expected address is `https://itu-5748.github.io/sleepy/` once deployment succeeds. Pages availability depends on repository visibility and the GitHub plan.

An original browser action roguelite with an angled 2.5D pixel view, inspired by fast arena combat and Z.H.P.-style permanent progression. No build step or third-party runtime dependencies.

## Play

Run `npm start` from this directory and open port 8080 in your browser. Desktop keyboard and mouse are required. WASD or arrow keys move, mouse aims, left click or J attacks, Space dashes, and Escape pauses.

Explore a continuous scrolling map across five regions and defeat the Ash Warden in the eastern sanctum. Enemies inhabit camps and pursue you nearby; paths remain open, so you can fight, retreat, or explore. Walk and dash along the winding road and connected combat clearings; surrounding terrain blocks movement and attacks. Branch paths lead to the optional shrines. Follow the road east and use the minimap to find four optional shrines. Press E nearby to choose a temporary blessing. At the end of each run, gain permanent vitality from damage endured (one point per 80 damage), might from kills (one per eight), and agility from attacks evaded during a dash (one per six). Each stat grants up to three points per run. Earn embers from kills and new regions reached, with a minimum of one per run; spend three to raise a stat of your choice. Base stats cap at 200 points each. Choose a blade, spear, or ranged orb before entering.

Progress is stored locally in your browser, without accounts or a backend. Clearing browser storage deletes it. Run upgrades reset on each life. Closing a running game abandons that run without settlement.

## Validate

`npm test` exercises permanent gains, settlement, spending, save recovery, map boundaries, camera tracking, and encounter resets. `node --check src/game.js` checks game syntax. Combat is a prototype: original pixel-art knight, poison-spitting spiders, and melee skeletons, five connected regions, one boss, no narrative or multiplayer, and no touch controls. Fonts fall back to system fonts if Google Fonts is unavailable.

Spiders spit green poison bolts at range (8 impact damage plus 12 poison damage over three seconds). Poison refreshes without stacking; dash through bolts to avoid it. Skeletons approach and telegraph a sword strike for 0.4 seconds before dealing 14 damage in melee range. Move out of reach or dash to evade. The Ash Warden is a crowned skeleton boss.

The 2.5D renderer keeps the existing world-space combat and collision, projecting it through a fixed angled camera. Roads have cached stone textures and extruded cliff faces; upright pixel sprites, trees, ruins, and glowing shrines are sorted by screen depth. WASD moves relative to the screen, and mouse aiming is converted back to world coordinates. Directional shadows, torch glow, and foreground tree fading improve depth and readability. All art is rendered procedurally; no engine or external art downloads are required.
