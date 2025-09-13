import { Link } from "react-router-dom";
import { userStore } from "../entities/user/model/userStore";

export default function Header() {
  const user = userStore((state) => state.user);

  return (
    <header className="w-full h-[70px] items-center bg-black/90 flex flex-row gap-[20px] px-[2rem]">
      <Link to={"/"} className="h-full flex items-center gap-[10px]">
        <img
          className="h-[80%] rounded-[16px] object-cover"
          src="https://play-lh.googleusercontent.com/2Sg4XierPqz0hVUA8rRNteutJhaUE4YMPQIN-wDIJ1x5piXeHA6G1-UWXj_n4R29F_o"
        />
        <span className="text-[1.5rem] text-white font-bold">inDrive</span>
      </Link>
      <nav className="w-full h-full flex flex-row justify-end items-center gap-[20px]">
        <Link
          to="/"
          className="text-white hover:text-gray-300 transition duration-300"
        >
          Главная
        </Link>
        <Link
          to="/about"
          className="text-white hover:text-gray-300 transition duration-300"
        >
          О нас
        </Link>
        <Link
          to={user ? "/car-verification" : "/auth?mode=registration"}
          className="text-white hover:text-gray-300 transition duration-300"
        >
          {user ? "Проверить машину" : "Зарегистрироваться"}
        </Link>
      </nav>
    </header>
  );
}
