import {Enemy} from './Enemy';

export class SearchResult{
    constructor(
        public userId?:string,
        public enemy?:Enemy
    ){}
}