import { Form } from "react-router-dom";
import { usePictures } from "../entities/picture/model/usePictures";
import LoadingSpinner from "../shared/loading-spinner/ui/LoadingSpinner";
import AppealList from "../features/appeal/ui/Appeal";
import { userStore } from "../entities/user/model/userStore";
import PictureList from "../entities/picture/ui/PictureList";

export default function AppealPage() {
  const { isLoading, isError, photos } = usePictures();

  const user = userStore((state) => state.user);

  return (
    <main className="min-h-full w-full px-[2rem] py-[2rem] flex flex-col gap-[2rem] items-center justify-start">
      <section className="flex flex-col items-center justify-center gap-[1rem]">
        <span className="text-[3rem]">
          Недовольны результатами последнего осмотра искуственного интеллекта?
        </span>
        <span className="text-[1.5rem]">
          Подайте апелляцию на решение системы.
        </span>
      </section>

      {isLoading && <LoadingSpinner />}
      {isError && <span>Ошибка загрузки фотографий.</span>}
      {!isLoading && !isError && <PictureList photos={photos} />}

      {!user?.appealId && (
        <section>
          <Form method="post" className="flex flex-col gap-[10px]">
            <textarea
              name="description"
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
      )}
    </main>
  );
}

export const action = async ({ request }: { request: Request }) => {
  const fd = await request.formData();
  const description = fd.get("description");
  console.log(description)
  const response = await fetch(`/api/appeals`, {
    credentials: "include",
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ description }),
  });

  if (response.ok) {
    return { success: true, message: "Апелляция успешно отправлена." };
  }

  return {
    success: false,
    message: "Ошибка при отправке апелляции. Пожалуйста, попробуйте еще раз.",
  };
};
