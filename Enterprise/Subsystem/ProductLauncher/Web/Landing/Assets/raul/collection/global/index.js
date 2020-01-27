import defineCommands from './commands';
import defineMisc from './misc';
import './modality';
const win = window;
win.RAUL = win.RAUL || {};
win.RAUL.version = 'RAUL_VERSION';
defineCommands(win);
defineMisc(win);
