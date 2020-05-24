import {Friend} from './Friend';
export class UserClient{
    constructor(
        public id?:string,
        public name?:string,
        public winCount?:Int16Array,
        public gold?:number,
        public imgAvatarHref?:string,
        public rank?:number,
        public friends?:Friend[]
    ){}
   
}