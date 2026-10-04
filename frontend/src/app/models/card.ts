import { presentModel } from './present';

export class cardModel {
    id!: number;
    presentId!: number;
    present?: presentModel;
    userId!: number;
    isPaid!: boolean;
    createdAt!: Date;
}