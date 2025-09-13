import { Link } from "react-router-dom";

export default function HomePage() {
  return (
    <main className="w-full h-full flex flex-row items-center justify-between flex-col bg-[#a7e92f] px-[50px]">
      <section className="w-full flex flex-col py-[20px] px-[20px] gap-[20px]">
        <div className="flex flex-row items-center gap-[10px]">
          <img
            className="h-[10rem] rounded-[16px] object-cover"
            src="https://play-lh.googleusercontent.com/2Sg4XierPqz0hVUA8rRNteutJhaUE4YMPQIN-wDIJ1x5piXeHA6G1-UWXj_n4R29F_o"
          />
          <span className="text-[5rem]">inDrive</span>
        </div>
        <div className="font-bold flex flex-col flex-center justify-center gap-[3rem]">
          <span className="text-[3rem]">Выбирайте своих пассажиров</span>
          <span className="text-[1.5rem] text-black/80">
            Ездите с выгодой и минимальной коммиссией
          </span>
        </div>
        <div className="flex flex-center justify-between font-bold">
          <Link
            to="/about"
            className="px-[20px] py-[20px] bg-white hover:bg-gray-300 text-black rounded-[6px] cursor-pointer w-[40%] text-center"
          >
            Про нас
          </Link>
          <Link
            to="/auth?mode=login"
            className="px-[20px] py-[20px] bg-black/90 hover:bg-black text-white rounded-[6px] cursor-pointer"
          >
            Войти или Зарегистрироваться
          </Link>
        </div>
      </section>
      <section className="w-full flex py-[20px] px-[20px]">
        <img
          className="object-cover rounded-[16px]"
          src="https://cdn.prod.website-files.com/63f494abd4ba494a0527f588/6679747b6c01b892e998a684_INDRIVE_BRAZIL_0298%201.jpg"
        />
      </section>
    </main>
  );
}
