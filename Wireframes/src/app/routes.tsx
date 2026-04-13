import { createBrowserRouter } from "react-router";
import { MainMenu } from "./components/MainMenu";
import { TrainingView } from "./components/TrainingView";
import { Tutorial } from "./components/Tutorial";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: MainMenu,
  },
  {
    path: "/training",
    Component: TrainingView,
  },
  {
    path: "/test",
    Component: TrainingView,
  },
  {
    path: "/tutorial",
    Component: Tutorial,
  },
]);
