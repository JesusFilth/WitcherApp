
export class GameManager{
    constructor(
        public cubesEnemy?:number[],
        public userGold?:number,
        public enemyGold?:number,
        public userWin?:number,
        public enemyWin?:number,
        public allBet?:number,
        public message?:string,
        public rollRaund?:number,
        public raund?:number,
        public bargainEnd?:boolean
    ){}
}