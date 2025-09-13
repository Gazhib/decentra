import { redirect, useNavigate, useSearchParams } from "react-router";
import Auth from "../features/authentication/Auth";
import { authPort } from "./ProtectedRoutes";
import { useEffect } from "react";
export default function AuthPage() {
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode");
  const isLogin = mode === "login";
  const isRegistration = mode === "registration";
  const navigate = useNavigate();
  useEffect(() => {
    if (!isLogin && !isRegistration) navigate("/auth?mode=login");
  }, [isLogin, isRegistration, navigate]);

  return (
    <main className="w-full h-full flex lg:flex-row items-center flex-col">
      <Auth mode={mode || ""} />
      <section className="flex-5 bg-[#a7e92f] h-full lg:flex items-center justify-center hidden ">
        <span className="text-black text-[7rem]">inDrive</span>
      </section>
    </main>
  );
}

export async function action({ request }: { request: Request }) {
  const fd = await request.formData();
  const url = new URL(request.url);
  const mode = url.searchParams.get("mode");
  const fetchUrl = `${authPort}/${mode}`;

  const login = fd.get("Login");
  const name = fd.get("Name");
  const surname = fd.get("Surname");
  const password = fd.get("Password");
  const confirmPassword = fd.get("Confirm password");

  if (mode === "login" || mode === "registration") {
    const response = await fetch(fetchUrl, {
      method: "POST",
      credentials: "include",
      body: JSON.stringify({ login, password, confirmPassword, name, surname }),
      headers: {
        "Content-Type": "application/json",
      },
    });

    const responseData = await response.json();

    if (response.ok) {
      return redirect("/chats");
    } else {
      return {
        message:
          responseData ||
          `${mode === "login" ? "Login" : "Registration"} failed`,
      };
    }
  } else {
    return redirect("/auth?mode=login");
  }
}
