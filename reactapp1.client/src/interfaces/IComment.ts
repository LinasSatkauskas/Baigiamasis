export interface IComment {
  id?: number
  plantId: number // NEW
  createdAt?: string
  updatedAt?: string | null
  email: string
  text: string
  isApproved?: boolean
}
