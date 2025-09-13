import { Link } from "react-router";

export default function ErrorPage() {
  return (
    <main className="bg-white h-full w-full text-black flex flex-col items-center justify-around text-[3rem]">
      <section>
        <span>Упс, что то пошло не так...</span>
      </section>
      <Link
        to="/"
        className="bg-[#a7e92f] border-[1px] border-[#333333] px-[20px] py-[10px] rounded-[6px] transition duration-300 hover:bg-[#8ec428]"
      >
        <span>Обратно на главную страницу</span>
      </Link>
    </main>
  );
}
