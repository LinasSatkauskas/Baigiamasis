export interface IComment {
    id?: number;
    plantId: number;   // NEW
    email: string;
    text: string;
    isApproved?: boolean;
}