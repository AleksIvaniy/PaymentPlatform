1.улучшим это ещё сильнее: уберём даже этот код
await _validator.ValidateAsync(...)
из каждого Controller и сделаем свой async validation filter. Тогда Controller снова станет чистым.