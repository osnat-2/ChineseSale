export class userModel {
    id!: number;
    name!: string;
    phone!: string;
    email!: string;
    password!: string;
    registrationTime!: Date
    role: string = "User";
    isActive!:boolean;
}
