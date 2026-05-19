export interface IUserItem {
  id: string
  email?: string | null
  userName?: string | null
  roles: string[]
  isCurrentUser: boolean
}
