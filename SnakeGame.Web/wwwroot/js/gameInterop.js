let activeCleanup = null;

const GAME_KEYS = new Set([
  "ArrowUp",
  "ArrowDown",
  "ArrowLeft",
  "ArrowRight",
  "w",
  "W",
  "a",
  "A",
  "s",
  "S",
  "d",
  "D",
  " ",
  "p",
  "P",
  "r",
  "R",
  "Enter",
  "Escape",
]);

function isEditableTarget(target) {
  if (!target || !(target instanceof Element)) {
    return false;
  }

  const tag = target.tagName;
  return (
    tag === "INPUT" ||
    tag === "SELECT" ||
    tag === "TEXTAREA" ||
    target.isContentEditable
  );
}

export function registerGameListeners(dotNetRef) {
  disposeGameListeners();

  const onKeyDown = (event) => {
    if (event.ctrlKey || event.metaKey || event.altKey) {
      return;
    }

    if (!GAME_KEYS.has(event.key)) {
      return;
    }

    // Preserve native interaction on form inputs and select dropdowns.
    if (isEditableTarget(event.target) && event.key !== "Escape") {
      return;
    }

    // Allow keyboard users who intentionally tabbed to a button to activate it natively.
    if (
      event.target instanceof Element &&
      event.target.tagName === "BUTTON" &&
      (event.key === "Enter" || event.key === " ")
    ) {
      return;
    }

    event.preventDefault();
    dotNetRef.invokeMethodAsync("HandleGlobalKeyDownAsync", event.key);
  };

  const onVisibilityChange = () => {
    if (document.hidden) {
      dotNetRef.invokeMethodAsync("HandleWindowInactiveAsync");
    }
  };

  const onWindowBlur = () => {
    dotNetRef.invokeMethodAsync("HandleWindowInactiveAsync");
  };

  window.addEventListener("keydown", onKeyDown, { passive: false });
  document.addEventListener("visibilitychange", onVisibilityChange);
  window.addEventListener("blur", onWindowBlur);

  activeCleanup = () => {
    window.removeEventListener("keydown", onKeyDown);
    document.removeEventListener("visibilitychange", onVisibilityChange);
    window.removeEventListener("blur", onWindowBlur);
  };
}

export function disposeGameListeners() {
  if (activeCleanup) {
    activeCleanup();
    activeCleanup = null;
  }
}
