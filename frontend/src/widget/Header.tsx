import { Link } from "react-router-dom";

export default function Header() {
  return (
    <header className="w-full h-[70px] items-center bg-black/90 flex flex-row gap-[20px] px-[20px]">
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
          to="/auth?mode=registration"
          className="text-white hover:text-gray-300 transition duration-300"
        >
          Зарегистрироваться
        </Link>
      </nav>
    </header>
  );
}
