import { useEffect, useState } from "react";
import { Navigate, Outlet, useNavigate } from "react-router";
import { userStore } from "../entities/user/model/userStore";
import { useUser } from "../entities/user/model/useUser";

export const port = import.meta.env.VITE_APP_DF_PORT;
export const authPort = import.meta.env.VITE_APP_AUTH_PORT;

export default function ProtectedRoutes() {
  const [checked, setChecked] = useState(false);
  const [isAuth, setIsAuth] = useState(false);
  const setUser = userStore((state) => state.setUser);
  const {} = useUser();

  const navigate = useNavigate();
  useEffect(() => {
    const handle = async () => {};
    handle();
  }, [navigate]);

  if (!checked) return null;
  return isAuth ? (
    <>
      <Outlet />
    </>
  ) : (
    <Navigate to="/auth?mode=login" />
  );
}
