namespace Train_asp_core
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public int UserGroupId { get; set; }

        public UserGroup? UserGroup { get; set; }


        public int UserStateId { get; set; }

        public UserState? UserState { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}\nLogin: {Login}\nPassword: {Password}\nUser Group Id: {UserGroupId}\nUser State Id: {UserStateId}\n";
        }
    }
}
