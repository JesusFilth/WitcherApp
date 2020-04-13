import {User} from './User';
import {Enemy} from './Enemy';

export class Opponents{
    constructor(
        public user?:User,
        public enemy?:Enemy
    ){}
}