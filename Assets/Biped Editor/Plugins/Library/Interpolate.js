#pragma strict
#pragma implicit
#pragma downcast

/**
 * Interpolation utility functions: easing, bezier, and catmull-rom.
 * Consider using Unity's Animation curve editor and AnimationCurve class
 * before scripting the desired behaviour using this utility.
 *
 * Interpolation functionality available at different levels of abstraction.
 * Low level access via individual easing functions (ex. EaseInOutCirc),
 * Bezier(), and CatmullRom(). High level access using sequence generators,
 * NewEase(), NewBezier(), and NewCatmullRom().
 *
 * Sequence generators are typically used as follows:
 *
 * var sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration);
 * for (var newPoint in sequence) {
 *   transform.position = newPoint;
 *   yield;
 * }
 *
 * Or:
 *
 * var sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration);
 * function Update() {
 *   if (sequence.MoveNext()) {
 *     transform.position = sequence.Current;
 *   }
 * }
 *
 * The low level functions work similarly to Unity's built in Lerp and it is
 * up to you to track and pass in elapsedTime and duration on every call. The
 * functions take this form (or the logical equivalent for Bezier() and
 * CatmullRom()).
 *
 * transform.position = ease(start, distance, elapsedTime, duration);
 *
 * For convenience in configuration you can use the Ease(EaseType) function to
 * look up a concrete easing function:
 *
 * var easeType : EaseType; // set using Unity's property inspector
 * var ease : Function; // easing of a particular EaseType
 * function Awake() {
 *   ease = Interpolate.Ease(easeType);
 * }
 *
 * @author Fernando Zapata (fernando@cpudreams.com)
 */

/**
 * Different methods of easing interpolation.
 */
enum EaseType {
  Linear,
  EaseInQuad,
  EaseOutQuad,
  EaseInOutQuad,
  EaseInCubic,
  EaseOutCubic,
  EaseInOutCubic,
  EaseInQuart,
  EaseOutQuart,
  EaseInOutQuart,
  EaseInQuint,
  EaseOutQuint,
  EaseInOutQuint,
  EaseInSine,
  EaseOutSine,
  EaseInOutSine,
  EaseInExpo,
  EaseOutExpo,
  EaseInOutExpo,
  EaseInCirc,
  EaseOutCirc,
  EaseInOutCirc
}

/**
 * Returns sequence generator from start to end over duration using the
 * given easing function. The sequence is generated as it is accessed
 * using the Time.deltaTime to calculate the portion of duration that has
 * elapsed.
 */
static function NewEase(ease : Function, start : Vector3,
                        end : Vector3, duration : float) : IEnumerator {
  var timer = NewTimer(duration);
  return NewEase(ease, start, end, duration, timer);
}

/**
 * Instead of easing based on time, generate n interpolated points (slices)
 * between the start and end positions.
 */
static function NewEase(ease : Function, start : Vector3,
                        end : Vector3, slices : int) : IEnumerator {
  var counter = NewCounter(0, slices + 1, 1);
  return NewEase(ease, start, end, slices + 1, counter);
}

/**
 * Generic easing sequence generator used to implement the time and
 * slice variants. Normally you would not use this function directly.
 */
static function NewEase(ease : Function, start : Vector3, end : Vector3,
                        total : float, driver : IEnumerator) : IEnumerator {
  var distance = end - start;
  for (var i in driver) {
    yield Ease(ease, start, distance, i, total);
  }
}

/**
 * Vector3 interpolation using given easing method. Easing is done independently
 * on all three vector axis.
 */
static function Ease(ease : Function,
                     start : Vector3, distance : Vector3,
                     elapsedTime : float, duration : float) : Vector3 {
  start.x = ease(start.x, distance.x, elapsedTime, duration);
  start.y = ease(start.y, distance.y, elapsedTime, duration);
  start.z = ease(start.z, distance.z, elapsedTime, duration);
  return start;
}

/**
 * Returns the static method that implements the given easing type for scalars.
 * Use this method to easily switch between easing interpolation types.
 *
 * All easing methods clamp elapsedTime so that it is always <= duration.
 *
 * var ease = Interpolate.Ease(EaseType.EaseInQuad);
 * i = ease(start, distance, elapsedTime, duration);
 */
static function Ease(type : EaseType) : Function {
  // Source Flash easing functions:
  // http://gizma.com/easing/
  // http://www.robertpenner.com/easing/easing_demo.html
  //
  // Changed to use more friendly variable names, that follow my Lerp
  // conventions:
  // start = b (start value)
  // distance = c (change in value)
  // elapsedTime = t (current time)
  // duration = d (time duration)

  var f : Function;
  switch (type) {
  case EaseType.Linear: f = Interpolate.Linear; break;
  case EaseType.EaseInQuad: f = Interpolate.EaseInQuad; break;
  case EaseType.EaseOutQuad: f = Interpolate.EaseOutQuad; break;
  case EaseType.EaseInOutQuad: f = Interpolate.EaseInOutQuad; break;
  case EaseType.EaseInCubic: f = Interpolate.EaseInCubic; break;
  case EaseType.EaseOutCubic: f = Interpolate.EaseOutCubic; break;
  case EaseType.EaseInOutCubic: f = Interpolate.EaseInOutCubic; break;
  case EaseType.EaseInQuart: f = Interpolate.EaseInQuart; break;
  case EaseType.EaseOutQuart: f = Interpolate.EaseOutQuart; break;
  case EaseType.EaseInOutQuart: f = Interpolate.EaseInOutQuart; break;
  case EaseType.EaseInQuint: f = Interpolate.EaseInQuint; break;
  case EaseType.EaseOutQuint: f = Interpolate.EaseOutQuint; break;
  case EaseType.EaseInOutQuint: f = Interpolate.EaseInOutQuint; break;
  case EaseType.EaseInSine: f = Interpolate.EaseInSine; break;
  case EaseType.EaseOutSine: f = Interpolate.EaseOutSine; break;
  case EaseType.EaseInOutSine: f = Interpolate.EaseInOutSine; break;
  case EaseType.EaseInExpo: f = Interpolate.EaseInExpo; break;
  case EaseType.EaseOutExpo: f = Interpolate.EaseOutExpo; break;
  case EaseType.EaseInOutExpo: f = Interpolate.EaseInOutExpo; break;
  case EaseType.EaseInCirc: f = Interpolate.EaseInCirc; break;
  case EaseType.EaseOutCirc: f = Interpolate.EaseOutCirc; break;
  case EaseType.EaseInOutCirc: f = Interpolate.EaseInOutCirc; break;
  }
  return f;
}

/**
 * Returns sequence generator from the first node to the last node over
 * duration time using the points in-between the first and last node
 * as control points of a bezier curve used to generate the interpolated points
 * in the sequence. If there are no control points (ie. only two nodes, first
 * and last) then this behaves exactly the same as NewEase(). In other words
 * a zero-degree bezier spline curve is just the easing method. The sequence
 * is generated as it is accessed using the Time.deltaTime to calculate the
 * portion of duration that has elapsed.
 */
static function NewBezier(ease : Function, nodes : Transform[],
                          duration : float) : IEnumerator {
  var timer = NewTimer(duration);
  return NewBezier(ease, nodes, TransformDotPosition, duration, timer);
}

/**
 * Instead of interpolating based on time, generate n interpolated points
 * (slices) between the first and last node.
 */
static function NewBezier(ease : Function, nodes : Transform[],
                          slices : int) : IEnumerator {
  var counter = NewCounter(0, slices + 1, 1);
  return NewBezier(ease, nodes, TransformDotPosition, slices + 1, counter);
}

/**
 * A Vector3[] variation of the Transform[] NewBezier() function.
 * Same functionality but using Vector3s to define bezier curve.
 */
static function NewBezier(ease : Function, points : Vector3[],
                          duration : float) : IEnumerator {
  var timer = NewTimer(duration);
  return NewBezier(ease, points, Identity, duration, timer);
}

/**
 * A Vector3[] variation of the Transform[] NewBezier() function.
 * Same functionality but using Vector3s to define bezier curve.
 */
static function NewBezier(ease : Function, points : Vector3[],
                          slices : int) : IEnumerator {
  var counter = NewCounter(0, slices + 1, 1);
  return NewBezier(ease, points, Identity, slices + 1, counter);
}

/**
 * Generic bezier spline sequence generator used to implement the time and
 * slice variants. Normally you would not use this function directly.
 */
static function NewBezier(ease : Function, nodes : IList, toVector3 : Function,
                          maxStep : float, steps : IEnumerator) : IEnumerator {
  // need at least two nodes to spline between
  if (nodes.Count >= 2) {
    // copy nodes array since Bezier is destructive
    var points = new Vector3[nodes.Count];

    for (var step in steps) {
      // re-initialize copy before each destructive call to Bezier
      for (var i = 0; i < nodes.Count; i++) {
        points[i] = toVector3(nodes[i]);
      }
      yield Bezier(ease, points, step, maxStep);
      // make sure last value is always generated
    }
  }
}

/**
 * A Vector3 n-degree bezier spline.
 *
 * WARNING: The points array is modified by Bezier. See NewBezier() for a
 * safe and user friendly alternative.
 *
 * You can pass zero control points, just the start and end points, for just
 * plain easing. In other words a zero-degree bezier spline curve is just the
 * easing method.
 *
 * @param points start point, n control points, end point
 */
static function Bezier(ease : Function, points : Vector3[],
                        elapsedTime : float, duration : float) : Vector3 {
  // Reference: http://ibiblio.org/e-notes/Splines/Bezier.htm
  // Interpolate the n starting points to generate the next j = (n - 1) points,
  // then interpolate those n - 1 points to generate the next n - 2 points,
  // continue this until we have generated the last point (n - (n - 1)), j = 1.
  // We store the next set of output points in the same array as the
  // input points used to generate them. This works because we store the
  // result in the slot of the input point that is no longer used for this
  // iteration.
  for (var j = points.length - 1; j > 0; j--) {
    for (var i = 0; i < j; i++) {
      points[i].x = ease(points[i].x, points[i + 1].x - points[i].x,
                         elapsedTime, duration);
      points[i].y = ease(points[i].y, points[i + 1].y - points[i].y,
                         elapsedTime, duration);
      points[i].z = ease(points[i].z, points[i + 1].z - points[i].z,
                         elapsedTime, duration);
    }
  }
  return points[0];
}

/**
 * Returns sequence generator from the first node, through each control point,
 * and to the last node. N points are generated between each node (slices)
 * using Catmull-Rom.
 */
static function NewCatmullRom(nodes : Transform[], slices : int,
                              loop : boolean) : IEnumerator {
  return NewCatmullRom(nodes, TransformDotPosition, slices, loop);
}

/**
 * A Vector3[] variation of the Transform[] NewCatmullRom() function.
 * Same functionality but using Vector3s to define curve.
 */
static function NewCatmullRom(points : Vector3[], slices : int,
                              loop : boolean) : IEnumerator {
  return NewCatmullRom(points, Identity, slices, loop);
}

/**
 * Generic catmull-rom spline sequence generator used to implement the
 * Vector3[] and Transform[] variants. Normally you would not use this
 * function directly.
 */
static function NewCatmullRom(nodes : IList, toVector3 : Function,
                              slices : int, loop : boolean) : IEnumerator {
  // need at least two nodes to spline between
  if (nodes.Count >= 2) {

    // yield the first point explicitly, if looping the first point
    // will be generated again in the step for loop when interpolating
    // from last point back to the first point
    yield toVector3(nodes[0]);

    var last = nodes.Count - 1;
    for (var current = 0; loop || current < last; current++) {
      // wrap around when looping
      if (loop && current > last) {
        current = 0;
      }
      // handle edge cases for looping and non-looping scenarios
      // when looping we wrap around, when not looping use start for previous
      // and end for next when you at the ends of the nodes array
      var previous = (current == 0) ? ((loop) ? last : current) : current - 1;
      var start = current;
      var end = (current == last) ? ((loop) ? 0 : current) : current + 1;
      var next = (end == last) ? ((loop) ? 0 : end) : end + 1;

      // adding one guarantees yielding at least the end point
      var stepCount = slices + 1;
      for (var step = 1; step <= stepCount; step++) {
        yield CatmullRom(toVector3(nodes[previous]),
                         toVector3(nodes[start]),
                         toVector3(nodes[end]),
                         toVector3(nodes[next]),
                         step, stepCount);
      }
    }
  }
}

/**
 * A Vector3 Catmull-Rom spline. Catmull-Rom splines are similar to bezier
 * splines but have the useful property that the generated curve will go
 * through each of the control points.
 *
 * NOTE: The NewCatmullRom() functions are an easier to use alternative to this
 * raw Catmull-Rom implementation.
 *
 * @param previous the point just before the start point or the start point
 *                 itself if no previous point is available
 * @param start generated when elapsedTime == 0
 * @param end generated when elapsedTime >= duration
 * @param next the point just after the end point or the end point itself if no
 *             next point is available
 */
static function CatmullRom(previous : Vector3, start : Vector3, end :
                           Vector3, next : Vector3, elapsedTime : float,
                           duration : float) : Vector3 {
  // References used:
  // p.266 GemsV1
  //
  // tension is often set to 0.5 but you can use any reasonable value:
  // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
  //
  // bias and tension controls:
  // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

  var percentComplete = elapsedTime / duration;
  var percentCompleteSquared = percentComplete * percentComplete;
  var percentCompleteCubed = percentCompleteSquared * percentComplete;

  return previous * (-0.5*percentCompleteCubed +
                     percentCompleteSquared -
                     0.5*percentComplete) +
    start * (1.5*percentCompleteCubed +
             -2.5*percentCompleteSquared + 1.0) +
    end * (-1.5*percentCompleteCubed +
           2.0*percentCompleteSquared +
           0.5*percentComplete) +
    next * (0.5*percentCompleteCubed -
            0.5*percentCompleteSquared);
}

/**
 * Sequence of eleapsedTimes until elapsedTime is >= duration.
 *
 * Note: elapsedTimes are calculated using the value of Time.deltatTime each
 * time a value is requested.
 */
static function NewTimer(duration : float) : IEnumerator {
  var elapsedTime = 0.0;
  while (elapsedTime < duration) {
    yield elapsedTime;
    elapsedTime += Time.deltaTime;
    // make sure last value is never skipped
    if (elapsedTime >= duration) {
      yield elapsedTime;
    }
  }
}

/**
 * Generates sequence of integers from start to end (inclusive) one step
 * at a time.
 */
static function NewCounter(start : int, end : int, step : int) : IEnumerator {
  for (var i = start; i <= end; i += step) {
    yield i;
  }
}

static function Identity(v : Vector3) : Vector3 {
  return v;
}

static function TransformDotPosition(t : Transform) : Vector3 {
  return t.position;
}

/**
 * Linear interpolation (same as Mathf.Lerp)
 */
static function Linear(start : float, distance : float,
                       elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return distance * (elapsedTime / duration) + start;
}

/**
 * quadratic easing in - accelerating from zero velocity
 */
static function EaseInQuad(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return distance*elapsedTime*elapsedTime + start;
}

/**
 * quadratic easing out - decelerating to zero velocity
 */
static function EaseOutQuad(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return -distance * elapsedTime*(elapsedTime-2) + start;
}

/**
 * quadratic easing in/out - acceleration until halfway, then deceleration
 */
static function EaseInOutQuad(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return distance/2*elapsedTime*elapsedTime + start;
  elapsedTime--;
  return -distance/2 * (elapsedTime*(elapsedTime-2) - 1) + start;
}

/**
 * cubic easing in - accelerating from zero velocity
 */
static function EaseInCubic(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return distance*elapsedTime*elapsedTime*elapsedTime + start;
}

/**
 * cubic easing out - decelerating to zero velocity
 */
static function EaseOutCubic(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  elapsedTime--;
  return distance*(elapsedTime*elapsedTime*elapsedTime + 1) + start;
}

/**
 * cubic easing in/out - acceleration until halfway, then deceleration
 */
static function EaseInOutCubic(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return distance/2*elapsedTime*elapsedTime*elapsedTime +
                         start;
  elapsedTime -= 2;
  return distance/2*(elapsedTime*elapsedTime*elapsedTime + 2) + start;
}

/**
 * quartic easing in - accelerating from zero velocity
 */
static function EaseInQuart(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return distance*elapsedTime*elapsedTime*elapsedTime*elapsedTime + start;
}

/**
 * quartic easing out - decelerating to zero velocity
 */
static function EaseOutQuart(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  elapsedTime--;
  return -distance * (elapsedTime*elapsedTime*elapsedTime*elapsedTime - 1) +
                         start;
}

/**
 * quartic easing in/out - acceleration until halfway, then deceleration
 */
static function EaseInOutQuart(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return distance/2*
                         elapsedTime*elapsedTime*elapsedTime*elapsedTime +
                         start;
  elapsedTime -= 2;
  return -distance/2 * (elapsedTime*elapsedTime*elapsedTime*elapsedTime - 2) +
                         start;
}


/**
 * quintic easing in - accelerating from zero velocity
 */
static function EaseInQuint(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return distance*elapsedTime*elapsedTime*elapsedTime*elapsedTime*elapsedTime +
                         start;
}

/**
 * quintic easing out - decelerating to zero velocity
 */
static function EaseOutQuint(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  elapsedTime--;
  return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime *
                     elapsedTime + 1) + start;
}

/**
 * quintic easing in/out - acceleration until halfway, then deceleration
 */
static function EaseInOutQuint(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return distance/2 * elapsedTime * elapsedTime *
                         elapsedTime * elapsedTime * elapsedTime + start;
  elapsedTime -= 2;
  return distance/2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime *
                       elapsedTime + 2) + start;
}

/**
 * sinusoidal easing in - accelerating from zero velocity
 */
static function EaseInSine(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return -distance * Mathf.Cos(elapsedTime/duration * (Mathf.PI/2)) +
                         distance + start;
}

/**
 * sinusoidal easing out - decelerating to zero velocity
 */
static function EaseOutSine(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return distance * Mathf.Sin(elapsedTime/duration * (Mathf.PI/2)) + start;
}

/**
 * sinusoidal easing in/out - accelerating until halfway, then decelerating
 */
static function EaseInOutSine(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return -distance/2 * (Mathf.Cos(Mathf.PI*elapsedTime/duration) - 1) + start;
}

/**
 * exponential easing in - accelerating from zero velocity
 */
static function EaseInExpo(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return distance * Mathf.Pow( 2, 10 * (elapsedTime/duration - 1) ) + start;
}

/**
 * exponential easing out - decelerating to zero velocity
 */
static function EaseOutExpo(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime to be <= duration
  if (elapsedTime > duration) { elapsedTime = duration; }
  return distance * ( -Mathf.Pow( 2, -10 * elapsedTime/duration ) + 1 ) + start;
}

/**
 * exponential easing in/out - accelerating until halfway, then decelerating
 */
static function EaseInOutExpo(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return distance/2 *
                         Mathf.Pow( 2, 10 * (elapsedTime - 1) ) + start;
  elapsedTime--;
  return distance/2 * ( -Mathf.Pow( 2, -10 * elapsedTime) + 2 ) + start;
}

/**
 * circular easing in - accelerating from zero velocity
 */
static function EaseInCirc(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  return -distance * (Mathf.Sqrt(1 - elapsedTime*elapsedTime) - 1) + start;
}

/**
 * circular easing out - decelerating to zero velocity
 */
static function EaseOutCirc(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 1.0 : elapsedTime / duration;
  elapsedTime--;
  return distance * Mathf.Sqrt(1 - elapsedTime*elapsedTime) + start;
}

/**
 * circular easing in/out - acceleration until halfway, then deceleration
 */
static function EaseInOutCirc(start : float, distance : float,
                     elapsedTime : float, duration : float) : float {
  // clamp elapsedTime so that it cannot be greater than duration
  elapsedTime = (elapsedTime > duration) ? 2.0 : elapsedTime / (duration / 2);
  if (elapsedTime < 1) return -distance/2 *
                         (Mathf.Sqrt(1 - elapsedTime*elapsedTime) - 1) + start;
  elapsedTime -= 2;
  return distance/2 * (Mathf.Sqrt(1 - elapsedTime*elapsedTime) + 1) + start;
}