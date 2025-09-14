import { Form } from "react-router-dom";

export default function AppealPage() {
  return (
    <main className="h-full w-full px-[2rem] py-[2rem] flex flex-col gap-[2rem] items-center justify-start">
      <section className="flex flex-col items-center justify-center gap-[1rem]">
        <span className="text-[3rem]">
          Недовольны результатами последнего осмотра искуственного интеллекта?
        </span>
        <span className="text-[1.5rem]">
          Подайте апелляцию на решение системы.
        </span>
      </section>
      <section>
        <span>Результаты вашего последнего осмотра</span>
        {/* an array of images from result */}
      </section>
      <section>
        <Form className="flex flex-col gap-[10px]">
          <textarea
            className="w-full h-[200px] p-[10px] border-2 border-gray-300 rounded-[8px] resize-none"
            placeholder="Опишите причину апелляции..."
          ></textarea>
          <button
            type="submit"
            className="bg-[#4CAF50] text-white py-[10px] px-[20px] rounded-[8px] hover:bg-[#45a049] transition cursor-pointer"
          >
            Отправить апелляцию
          </button>
        </Form>
      </section>
    </main>
  );
}

export const loader = async () => {
  const response = await fetch(`/api/get-photos`, {
    credentials: "include",
  });

  if (response.ok) {
    const data = await response.json();
    return data;
  }
  return { photos: [] };
};
