import {Component} from '@angular/core';
import {Router} from '@angular/router';

import {Opponents} from '../../models/Opponents';

@Component({
    selector:'app-game',
    templateUrl:'./gameProcess.component.html',
    styleUrls:['./gameProcess.component.css']
})
export class GameProcessComponent{

    opponents:Opponents = new Opponents();
    constructor(private router:Router){
        this.opponents = router.getCurrentNavigation().extras.state;
        console.log(this.opponents);
    }
}