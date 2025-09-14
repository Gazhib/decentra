import { Navigate, Outlet } from "react-router";
import { useUser } from "../entities/user/model/useUser";
import { userStore } from "../entities/user/model/userStore";

export const port = import.meta.env.VITE_APP_DF_PORT;

export default function AuthProtectedRoutes() {
  const { isLoading } = useUser();

  const user = userStore((state) => state.user);

  if (isLoading) {
    return <div>Loading...</div>;
  }
  if (Boolean(user)) {
    return <Navigate to="/" />;
  }

  return <Outlet />;
}
