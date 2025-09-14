import { Navigate, Outlet } from "react-router";
import { useUser } from "../entities/user/model/useUser";
import { userStore } from "../entities/user/model/userStore";

export default function ProtectedRoutes() {
  const { isLoading } = useUser();

  const user = userStore((state) => state.user);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!user) {
    <Navigate to="/auth?mode=login" />;
  }

  return <Outlet />;
}
