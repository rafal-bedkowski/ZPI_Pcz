import { useState } from 'react';

export default function Counter() {
  const [count, setCount] = useState(0);

  return (
    <>
      <h1>Counter</h1>
      <p>Przykład komponentu ze stanem lokalnym, działającego wyłącznie w przeglądarce.</p>
      <p role="status">Aktualna wartość: {count}</p>
      <button type="button" onClick={() => setCount((value) => value + 1)}>Zwiększ</button>
    </>
  );
}
