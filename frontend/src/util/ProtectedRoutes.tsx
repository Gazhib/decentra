import { Navigate, Outlet } from "react-router";
import { userStore } from "../entities/user/model/userStore";

export const port = import.meta.env.VITE_APP_DF_PORT;

export default function ProtectedRoutes() {
  const user = userStore((state) => state.user);

  return user ? (
    <>
      <Outlet />
    </>
  ) : (
    <Navigate to="/auth?mode=login" />
  );
}
