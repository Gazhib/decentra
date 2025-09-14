import { Link } from "react-router-dom";
import PictureList from "../../../entities/picture/ui/PictureList";
import { userStore } from "../../../entities/user/model/userStore";
import type { Photo } from "../../../entities/picture/ui/Picture";
import LoadingSpinner from "../../../shared/loading-spinner/ui/LoadingSpinner";

export default function AppealList({
  isLoading,
  photos,
}: {
  isLoading: boolean;
  photos: Photo[];
}) {
  const user = userStore((state) => state.user);
  return !isLoading ? (
    <>
      <div className="w-full flex items-center gap-3 my-6">
        <span className="text-xl font-semibold text-gray-800">
          Последняя проверка:
        </span>
        <div className="flex-1 h-px bg-gray-200"></div>
      </div>
      <PictureList photos={photos} />
      {!user?.appealId ? (
        <Link
          to="/appeal"
          className="inline-flex items-center justify-center rounded-lg bg-blue-600 px-5 py-2.5 text-white shadow-sm transition-colors hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Подать на апеляцию
        </Link>
      ) : (
        user?.appealId && (
          <div
            className="inline-flex items-center gap-2 rounded-lg border border-blue-200 bg-blue-50 px-4 py-2 text-blue-800"
            role="status"
            aria-live="polite"
          >
            <span className="text-sm font-medium">
              Анализируем вашу аппеляцую. Ждите ответа
            </span>
          </div>
        )
      )}
    </>
  ) : (
    <LoadingSpinner />
  );
}
