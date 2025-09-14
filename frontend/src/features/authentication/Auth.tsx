import { Form, Link, useActionData, useNavigation } from "react-router";
import AuthInput from "./AuthInput";
import { motion } from "framer-motion";
type Props = {
  mode: string;
};

export default function Auth({ mode }: Props) {
  const isLogin = mode === "login";
  const errorMessage = useActionData();
  const navigation = useNavigation();
  const isSubmitting = navigation.state === "submitting";
  return (
    <Form
      method="post"
      className="flex-4 h-full w-full flex items-center justify-center px-[30px]"
    >
      <div className="w-[500px] gap-[30px] h-full flex flex-col justify-center">
        <header className="flex flex-col items-center text-center justify-center">
          <span className="text-[34px]">Добро пожаловать</span>
          <span className="text-[14px] text-[#636364]">
            Добро пожаловать! Введите свои данные{" "}
            {!isLogin && "для регистрации"} ниже.
          </span>
        </header>
        <section className="flex flex-col items-center gap-[10px]">
          <AuthInput placeholder="Введите номер" name="Login" label="Номер" />
          {!isLogin && (
            <>
              <AuthInput
                placeholder="Введите Имя"
                name="Name"
                label="Имя"
                type="text"
              />
              <AuthInput
                placeholder="Введите Фамилию"
                name="Surname"
                label="Фамилия"
                type="text"
              />
            </>
          )}
          <AuthInput
            placeholder="Введите пароль"
            name="Password"
            label="Пароль"
            type="password"
          />
          {!isLogin && (
            <AuthInput
              placeholder="Подтвердите пароль"
              name="Confirm password"
              label="Подтвердите пароль"
              type="password"
            />
          )}
          {errorMessage && (
            <span className="text-red-600">{errorMessage.message}</span>
          )}
        </section>
        <motion.footer
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="flex flex-col w-full gap-[20px]"
        >
          <section className="w-full justify-center items-center">
            <button
              style={{
                backgroundColor: isSubmitting ? "#000000B3" : "",
                color: isSubmitting ? "#E5E7EB" : "",
              }}
              disabled={isSubmitting}
              className="rounded-[10px] h-[40px] cursor-pointer w-full bg-[#a7e92f] hover:bg-[#8ec428] text-black hover:text-white"
            >
              {isLogin
                ? isSubmitting
                  ? "Входим..."
                  : "Войти"
                : isSubmitting
                ? "Регистрируемся..."
                : "Зарегистрироваться"}
            </button>
          </section>
          <span className="flex flex-row gap-[5px]">
            {isLogin ? "Нет" : "Есть"} аккаунт{isLogin ? "a" : ""}?
            <Link
              className="underline hover:text-gray-600"
              to={`/auth?mode=${isLogin ? "registration" : "login"}`}
            >
              {!isLogin
                ? "Войти в аккаунт"
                : "Зарегистрироваться меньше чем за 5 минут!"}
            </Link>
          </span>
        </motion.footer>
      </div>
    </Form>
  );
}
