import { useNavigate } from "react-router-dom";
import { userStore } from "../entities/user/model/userStore";
import { port } from "../util/ProtectedRoutes";

export default function AccountPage() {
  const user = userStore((state) => state.user);
  const setUser = userStore((state) => state.setUser);

  const navigate = useNavigate();

  const handleLogout = async () => {
    await fetch(`${port}/logout`, {
      credentials: "include",
    });
    setUser(null);
    navigate("/");
  };

  return (
    <main className="h-full w-full flex flex-col items-center justify-start gap-[20px] bg-white px-[50px] py-[20px] overflow-x-hidden">
      <h1 className="text-3xl font-bold mb-4">Аккаунт</h1>
      <section className="flex flex-col items-center justify-center gap-[10px]">
        <span className="text-[1.5rem] font-bold">
          Информация о пользователе
        </span>
        <div className="flex flex-col gap-[10px]">
          <div>
            <strong>Имя: </strong>{" "}
            <span>
              {user?.name} {user?.surname}
            </span>
          </div>
          <div>
            <strong>id: </strong> {user?.id}
          </div>
          <div>
            <strong>Дата последней проверки: </strong> {user?.lastDate}
          </div>
          <div>
            <strong>Телефон: </strong> {user?.phoneNumber}
          </div>
        </div>
      </section>
      <footer>
        <button
          onClick={handleLogout}
          className="bg-red-600 hover:bg-red-400 px-[2rem] py-[1rem] rounded-[16px] text-white cursor-pointer"
        >
          Выйти с аккаунта
        </button>
      </footer>
    </main>
  );
}
