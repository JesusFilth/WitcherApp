import {Friend} from './Friend';
export class User{
    constructor(
        public name?:string,
        public winCount?:Int16Array,
        public gold?:Int32Array,
        public imgAvatarHref?:string,
        public rank?:Int16Array,
        public friends?:Friend[]
    ){}
   
}