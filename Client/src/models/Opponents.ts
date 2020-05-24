import {UserClient} from './UserClient';
import {Enemy} from './Enemy';

export class Opponents{
    constructor(
        public user?:UserClient,
        public enemy?:Enemy
    ){}
}